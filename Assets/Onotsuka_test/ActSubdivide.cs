using System;
using System.Timers;
using System.Collections.Generic;
using UnityEngine;
public class ActSubdivide : MonoBehaviour {
    // キャスト時の丸め誤差をどこまで許容するか
    private static ConversionMode mode = ConversionMode.Mode1;

    // メッシュ情報を格納する
    private static Mesh originMesh;
    private static Vector3[] originVertices;
    private static Vector3[] originNormals;
    private static Vector2[] originUVs;
    private static MeshContainer mesh_right = new MeshContainer();
    private static MeshContainer mesh_left = new MeshContainer();

    // ローカル平面用
    private static Plane localPlane;

    // 切断対象のメッシュの頂点を左右判定して格納する
    private static bool[] vertexTruthValues;
    // 切断前の頂点番号が，切断後の頂点番号にどのように変換されるかを格納する
    private static int[] tracker;
    // 新頂点の両サイドの左右振り分け後の頂点番号を格納する
    private static Dictionary<int, (int, int)> sandwichVertices = new Dictionary<int, (int, int)>();

    // 切断されたポリゴンが生成される毎にマージ判定しながら格納する
    private static FusionPolygonList fusionPolygonList = new FusionPolygonList();
    // 切断辺が生成される毎にマージ判定をして，連結辺を生成しながら格納する
    private static SlashSurfaceGeometry slashSurfaceGeometry = new SlashSurfaceGeometry();

    // メインメソッド
    public static void CallSlash(
        GameObject targetGameObject,
        Plane cutter,
        Material cutSurfaceMaterial = null
    ) {
        Mesh mesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform transform = targetGameObject.transform;
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();

        Material[] mats = renderer.sharedMaterials;
        // 切断前のマテリアルが切断面用マテリアルでなければ，マテリアルを割り当てる
        if (cutSurfaceMaterial != null) {
            if (mats[mats.Length - 1]?.name == cutSurfaceMaterial.name) {
                addNewMaterial = false;
            } else {
                addNewMaterial = true;
            }
        } else {
            addNewMaterial = false;
        }
        // メインメソッド実行
        (Mesh copyMesh, Mesh originMesh) = CallSlash(mesh, transform, cutter, addNewMaterial);

        if (originMesh == null || copyMesh == null) {
            return;
        }
        // 必要であれば切断面用マテリアルを追加する
        if (addNewMaterial) {
            int matLength = mats.Length;
            Material[] newMats = new Material[matLength + 1];
            mats.CopyTo(newMats, 0);
            newMats[matLength] = cutSurfaceMaterial;

            renderer.sharedMaterials = newMats;
        }

        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;
        Transform originTransform = targetGameObject.transform;
        GameObject polygonInfo_subject = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        polygonInfo_subject.transform.parent = null;
        polygonInfo_subject.GetComponent<MeshFilter>().mesh = copyMesh;
        polygonInfo_subject.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;

        if (targetGameObject.GetComponent<MeshCollider>()) {
            targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
            polygonInfo_subject.GetComponent<MeshCollider>().sharedMesh = copyMesh;
        }
        return;
    }

    // メインメソッド
    public static (
        Mesh frontside,
        Mesh backside
    ) CallSlash(
        Mesh targetMesh,
        Transform targetTransform,
        Plane cutter,
        bool addNewMeshIndices = false
    ) {

        if (cutter.normal == Vector3.zero) {
            Debug.LogError("平面が平行です");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }

        // 初期化する
        {
            originMesh = targetMesh;
            originVertices = originMesh.vertices;
            originNormals = originMesh.normals;
            originUVs = originMesh.uv;

            int verticesLength = originVertices.Length;

            tracker = new int[verticesLength];
            vertexTruthValues = new bool[verticesLength];
            sandwichVertices.Clear();
            slashSurfaceGeometry.Clear();
            fusionPolygonList.Clear();

            mesh_right.Clear();
            mesh_left.Clear();
        }

        Vector3 scale = targetTransform.localScale;
        Vector3 pointOnPlane = cutter.normal * cutter.distance;

        localPlane.normal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(cutter.normal)).normalized;
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(pointOnPlane);
        localPlane.distance = Vector3.Dot(localPlane.normal, anchor);
        Plane plane = new Plane(localPlane.normal, localPlane.distance);

        // 切断前頂点の左右判定を行う
        int rightHut = 0;
        int leftHut = 0;

        // もとの頂点情報に左右情報を格納すると同時に，頂点情報を追加する
        for (int i = 0; i < originVertices.Length; i++) {
            vertexTruthValues[i] = plane.GetSide(originVertices[i]);

            if (vertexTruthValues[i]) {
                mesh_right.AddVertex(
                    originVertices[i],
                    originNormals[i],
                    originUVs[i]
                );
                tracker[i] = rightHut++;
            } else {
                mesh_left.AddVertex(
                    originVertices[i],
                    originNormals[i],
                    originUVs[i]
                );
                tracker[i] = leftHut++;
            }
        }
        if (rightHut < 4 || leftHut < 4) {
            return (null, null);
        }

        // サブメッシュの数だけループ
        for (int submeshDepartment = 0; submeshDepartment < originMesh.subMeshCount; submeshDepartment++) {
            // このサブメッシュの頂点数を取得する
            int[] submeshIndicesAry = originMesh.GetIndices(submeshDepartment);
            mesh_left.submesh.Add(new List<int>());
            mesh_right.submesh.Add(new List<int>());

            // サブメッシュの頂点数だけループ
            for (int i = 0; i < submeshIndicesAry.Length; i += 3) {
                int subIndex1 = submeshIndicesAry[i];
                int subIndex2 = submeshIndicesAry[i + 1];
                int subIndex3 = submeshIndicesAry[i + 2];
                bool subIndexRTLF1 = vertexTruthValues[subIndex1];
                bool subIndexRTLF2 = vertexTruthValues[subIndex2];
                bool subIndexRTLF3 = vertexTruthValues[subIndex3];

                //対象の三角形ポリゴンの頂点すべてが右側にある場合
                if (subIndexRTLF1 && subIndexRTLF2 && subIndexRTLF3) {
                    mesh_right.AddSubIndices(
                        submeshDepartment,
                        tracker[subIndex1],
                        tracker[subIndex2],
                        tracker[subIndex3]
                    );
                }
                // 対象の三角形ポリゴンの頂点すべてが左側にある場合
                else if (!subIndexRTLF1 && !subIndexRTLF2 && !subIndexRTLF3) {
                    mesh_left.AddSubIndices(
                        submeshDepartment,
                        tracker[subIndex1],
                        tracker[subIndex2],
                        tracker[subIndex3]
                    );
                }
                // 対象の三角形ポリゴンの頂点が左右に分かれている場合
                else {
                    ProcessMixedTriangle(
                        submeshDepartment,
                        new bool[3] { subIndexRTLF1, subIndexRTLF2, subIndexRTLF3 },
                        new int[3] { subIndex1, subIndex2, subIndex3 }
                    );
                }
            }
        }
        fusionPolygonList.MakeTriangles();//切断されたポリゴンはここでそれぞれのMeshに追加される

        // 切断面のマテリアルを追加する
        if (addNewMeshIndices) {
            mesh_right.submesh.Add(new List<int>());
            mesh_left.submesh.Add(new List<int>());
        }
        slashSurfaceGeometry.MakeCutSurface(mesh_right.submesh.Count - 1, targetTransform);

        //2つのMeshを新規に作ってそれぞれに情報を追加して出力する
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh _leftTracker";

        frontMesh.vertices = mesh_right.vertices.ToArray();
        frontMesh.normals = mesh_right.normals.ToArray();
        frontMesh.uv = mesh_right.uvs.ToArray();

        frontMesh.subMeshCount = mesh_right.submesh.Count;
        for (int i = 0; i < mesh_right.submesh.Count; i++) {
            frontMesh.SetIndices(mesh_right.submesh[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh _rightTracker";
        backMesh.vertices = mesh_left.vertices.ToArray();
        backMesh.normals = mesh_left.normals.ToArray();
        backMesh.uv = mesh_left.uvs.ToArray();

        backMesh.subMeshCount = mesh_left.submesh.Count;
        for (int i = 0; i < mesh_left.submesh.Count; i++) {
            backMesh.SetIndices(mesh_left.submesh[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        return (frontMesh, backMesh);
    }

    // 切断面上メッシュ情報を挿入する
    private static void ProcessMixedTriangle(
        int submeshDepartment,
        bool[] vertexTruthValues,
        int[] subIndices
    ) {
        ( // ポリゴンの頂点情報を扱いやすいように整理する
            bool rtlf,
            int right_toward,
            int right_away,
            int left_toward,
            int left_away
        ) = SortIndex(
            vertexTruthValues,
            subIndices[0],
            subIndices[1],
            subIndices[2]
        );

        Vector3 right_toward_vec, right_away_vec, left_toward_vec, left_away_vec;
        if (rtlf) {
            right_toward_vec = right_away_vec = originVertices[right_toward];
            left_toward_vec = originVertices[left_toward];
            left_away_vec = originVertices[left_away];
        } else {
            right_toward_vec = originVertices[right_toward];
            right_away_vec = originVertices[right_away];
            left_toward_vec = left_away_vec = originVertices[left_toward];
        }

        float ratio_toward = (localPlane.distance - Vector3.Dot(localPlane.normal, right_toward_vec)) / (Vector3.Dot(localPlane.normal, left_toward_vec - right_toward_vec));
        float ratio_away = (localPlane.distance - Vector3.Dot(localPlane.normal, right_away_vec)) / (Vector3.Dot(localPlane.normal, left_away_vec - right_away_vec));

        Vector3 newVertex_toward = Vector3.Lerp(right_toward_vec, left_toward_vec, ratio_toward);
        Vector3 newVertex_away = Vector3.Lerp(right_away_vec, left_away_vec, ratio_away);
        int edgeDirection = ToIntFromVector3((newVertex_away - newVertex_toward).normalized);

        // 新頂点情報を生成する
        VertexInfo vertexInfoToward = new VertexInfo(
            tracker[right_toward],
            tracker[left_toward],
            ratio_toward,
            newVertex_toward
        );
        VertexInfo vertexInfoAway = new VertexInfo(
            tracker[right_away],
            tracker[left_away],
            ratio_away,
            newVertex_away
        );
        // 新ポリゴン情報を生成する
        PolygonInfo polygonInfo_subject = new PolygonInfo(
            rtlf,
            submeshDepartment,
            edgeDirection,
            vertexInfoToward,
            vertexInfoAway
        );
        // 新ポリゴンの切断辺方向が既存の新ポリゴンの切断辺方向と同じ（同一平面）であれば，マージする
        fusionPolygonList.Add(edgeDirection, polygonInfo_subject);
    }

    // ポリゴンの頂点番号を並び替える
    public static (
        bool rtlf,
        int right_toward,
        int right_away,
        int left_toward,
        int left_away
    ) SortIndex(
        bool[] vertexTruthValues,
        int submeshIndex1,
        int submeshIndex2,
        int submeshIndex3
    ) {
        bool rtlf;

        if (vertexTruthValues[0]) {
            // t|t|f
            if (vertexTruthValues[1]) {
                rtlf = false;
                return (rtlf, submeshIndex2, submeshIndex1, submeshIndex3, submeshIndex3);
            } else {
                // t|f|t
                if (vertexTruthValues[2]) {
                    rtlf = false;
                    return (rtlf, submeshIndex1, submeshIndex3, submeshIndex2, submeshIndex2);
                }
                // t|f|f
                else {
                    rtlf = true;
                    return (rtlf, submeshIndex1, submeshIndex1, submeshIndex2, submeshIndex3);
                }
            }
        } else {
            if (vertexTruthValues[1]) {
                // f|t|t
                if (vertexTruthValues[2]) {
                    rtlf = false;
                    return (rtlf, submeshIndex3, submeshIndex2, submeshIndex1, submeshIndex1);
                }
                // f|t|f
                else {
                    rtlf = true;
                    return (rtlf, submeshIndex2, submeshIndex2, submeshIndex3, submeshIndex1);
                }
            }
            // f|f|t
            else {
                rtlf = true;
                return (rtlf, submeshIndex3, submeshIndex3, submeshIndex1, submeshIndex2);
            }
        }
    }

    // Vector3 から int にデータを変換する
    public static int ToIntFromVector3(Vector3 vec) {
        int filter;
        int amp;

        // モードに基づいてフィルターとアンプを決定
        switch (mode) {
            case ConversionMode.Mode1:
                filter = 0x000003FF;
                amp = 1 << 10;
                break;

            case ConversionMode.Mode2:
                filter = 0x000000FF;
                amp = 1 << 8;
                break;

            case ConversionMode.Mode3:
                filter = 0x0000FFFF;
                amp = 1 << 15;
                break;

            default:
                filter = 0x000003FF;
                amp = 1 << 10;
                break;
        }

        // X, Y, Z 座標をそれぞれ処理
        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }

    // メッシュの情報を格納する
    public class MeshContainer {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<List<int>> submesh = new List<List<int>>();

        public void Clear() {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            submesh.Clear();
        }

        // 切断基のオブジェクトのメッシュ情報をそのまま追加する用
        public void AddMesh(
            int submeshDepartment,
            int index1,
            int index2,
            int index3
        ) {
            int indexCount = vertices.Count;

            for (int i = 0; i < 3; i++) {
                submesh[submeshDepartment].Add(indexCount + i);
            }
            vertices.AddRange(new Vector3[] {
                originVertices[index1],
                originVertices[index2],
                originVertices[index3]
            });
            normals.AddRange(new Vector3[] {
                originNormals[index1],
                originNormals[index2],
                originNormals[index3]
            });
            uvs.AddRange(new Vector2[] {
                originUVs[index1],
                originUVs[index2],
                originUVs[index3]
            });
        }

        // 新しく作成したメッシュ情報を追加する用
        public void AddMesh(
            int submeshDepartment,
            Vector3 face,
            Vector3[] newVertices,
            Vector3[] newNormals,
            Vector2[] newUVs
        ) {
            int indexCount = vertices.Count;
            int sequence1 = 0;
            int sequence2 = 1;
            int sequence3 = 2;

            Vector3 calNormal = Vector3.Cross(
                newVertices[1] - newVertices[0],
                newVertices[2] - newVertices[0]
            ).normalized;

            if (Vector3.Dot(calNormal, face) < 0) {
                sequence1 = 2;
                sequence2 = 1;
                sequence3 = 0;
            }

            for (int i = 0; i < 3; i++) {
                submesh[submeshDepartment].Add(indexCount + i);
            }
            vertices.AddRange(new Vector3[] {
                newVertices[sequence1],
                newVertices[sequence2],
                newVertices[sequence3]
            });
            normals.AddRange(new Vector3[] {
                newNormals[sequence1],
                newNormals[sequence2],
                newNormals[sequence3]
            });
            uvs.AddRange(new Vector2[] {
                newUVs[sequence1],
                newUVs[sequence2],
                newUVs[sequence3]
            });
        }

        public void AddVertex(
            Vector3 vertex,
            Vector3 normal,
            Vector2 uv
        ) {
            vertices.Add(vertex);
            normals.Add(normal);
            uvs.Add(uv);
        }

        public void AddSubIndices(
            int submeshDepartment,
            int index1,
            int index2,
            int index3
        ) {
            submesh[submeshDepartment].AddRange(new int[] {
                index1,
                index2,
                index3
            });
        }
    }

    // 新頂点の情報を格納する
    public class VertexInfo {
        public int indexInLeftMesh, indexInRightMesh;
        public float ratio;
        public int vertexDomein;
        public Vector3 position;

        public VertexInfo(
            int _leftTracker,
            int _rightTracker,
            float _ratio,
            Vector3 _position
        ) {
            indexInLeftMesh = _leftTracker;
            indexInRightMesh = _rightTracker;
            vertexDomein = (_leftTracker << 16) | _rightTracker;
            ratio = _ratio;
            position = _position;
        }

        public (int index_right, int index_left) GetIndex() {
            Vector3 rightNormal, leftNormal;
            Vector2 rightUV, leftUV;

            rightNormal = mesh_right.normals[indexInLeftMesh];
            rightUV = mesh_right.uvs[indexInLeftMesh];
            leftNormal = mesh_left.normals[indexInRightMesh];
            leftUV = mesh_left.uvs[indexInRightMesh];

            Vector3 newNormal = Vector3.Lerp(rightNormal, leftNormal, ratio);
            Vector2 newUV = Vector2.Lerp(rightUV, leftUV, ratio);

            int index_right, index_left;
            (int, int) pair;

            // 既に同じ頂点が登録されている場合はその情報を使う
            if (sandwichVertices.TryGetValue(vertexDomein, out pair)) {
                index_right = pair.Item1;
                index_left = pair.Item2;
            } else {
                index_right = mesh_right.vertices.Count;
                mesh_right.AddVertex(
                    position,
                    newNormal,
                    newUV
                );
                index_left = mesh_left.vertices.Count;
                mesh_left.AddVertex(
                    position,
                    newNormal,
                    newUV
                );
                sandwichVertices.Add(vertexDomein, (index_right, index_left));
            }
            return (index_right, index_left);
        }
    }

    // 新ポリゴン候補の情報を格納する
    public class PolygonInfo {
        public int submeshDepartment;
        public int edgeDirection;
        public VertexInfo vertex_toward, vertex_away;
        public Node<int> internal_right, terminal_right, internal_left, terminal_left;
        public int count_rightVert, count_leftVert;

        public PolygonInfo(
            bool _rtlf,
            int _submeshDepartment,
            int _edgeDirection,
            VertexInfo _vertex_toward,
            VertexInfo _vertex_away
        ) {
            submeshDepartment = _submeshDepartment;
            edgeDirection = _edgeDirection;
            vertex_toward = _vertex_toward;
            vertex_away = _vertex_away;

            if (_rtlf) {
                internal_right = new Node<int>(_vertex_toward.indexInLeftMesh);
                terminal_right = internal_right;
                internal_left = new Node<int>(vertex_toward.indexInRightMesh);
                terminal_left = new Node<int>(vertex_away.indexInRightMesh);
                internal_left.next = terminal_left;
                count_rightVert = 1;
                count_leftVert = 2;
            } else {
                internal_right = new Node<int>(_vertex_toward.indexInLeftMesh);
                terminal_right = new Node<int>(_vertex_away.indexInLeftMesh);
                internal_right.next = terminal_right;
                internal_left = new Node<int>(vertex_toward.indexInRightMesh);
                terminal_left = internal_left;
                count_rightVert = 2;
                count_leftVert = 1;
            }
        }

        public void AddTriangleCirculation() {
            (int index_r_t, int index_l_t) = vertex_toward.GetIndex();
            (int index_r_a, int index_l_a) = vertex_away.GetIndex();

            Node<int> node;
            int index_previous, count, count_half;

            node = internal_right;
            index_previous = node.index;
            count = count_rightVert;
            count_half = count_rightVert / 2;

            // 右側のポリゴンのうち，vertex_toward 寄りの頂点（internal から 中間地点）までを，vertex_toward との辺を構成する三角形に分割しながら追加していく
            for (int i = 0; i < count_half; i++) {
                node = node.next;
                int index = node.index;
                mesh_right.AddSubIndices(
                    submeshDepartment,
                    index,
                    index_previous,
                    index_r_t
                );
                index_previous = index;
            }
            // 右側のポリゴンの頂点群の中間点と，vertex_toward, vertex_away で構成される三角形を追加する
            mesh_right.AddSubIndices(
                submeshDepartment,
                index_previous,
                index_r_t,
                index_r_a
            );
            // 右側のポリゴンのうち，vertex_away 寄りの頂点（中間地点から terminal）までを，vertex_away との辺を構成する三角形に分割しながら追加していく
            int elseCount = count_rightVert - count_half - 1;
            for (int i = 0; i < elseCount; i++) {
                node = node.next;
                int index = node.index;
                mesh_right.AddSubIndices(
                    submeshDepartment,
                    index,
                    index_previous,
                    index_r_a
                );
                index_previous = index;
            }

            node = internal_left;
            index_previous = node.index;
            count = count_leftVert;
            count_half = count_leftVert / 2;

            // 左側のポリゴンのうち，vertex_toward 寄りの頂点（internal から 中間地点）までを，vertex_toward との辺を構成する三角形に分割しながら追加していく
            for (int i = 0; i < count_half; i++) {
                node = node.next;
                int index = node.index;
                mesh_left.AddSubIndices(
                    submeshDepartment,
                    index,
                    index_l_t,
                    index_previous
                );
                index_previous = index;
            }
            // 左側のポリゴンの頂点群の中間点と，vertex_toward, vertex_away で構成される三角形を追加する
            mesh_left.AddSubIndices(
                submeshDepartment,
                index_previous,
                index_l_a,
                index_l_t
            );
            // 左側のポリゴンのうち，vertex_away 寄りの頂点（中間地点から terminal）までを，vertex_away との辺を構成する三角形に分割しながら追加していく
            elseCount = count_leftVert - count_half - 1;
            for (int i = 0; i < elseCount; i++) {
                node = node.next;
                int index = node.index;
                mesh_left.AddSubIndices(
                    submeshDepartment,
                    index,
                    index_l_a,
                    index_previous
                );
                index_previous = index;
            }
            // 切断平面を構成する図形の連結辺を生成を行ってもらうために随時追加していく
            slashSurfaceGeometry.Add(vertex_toward.position, vertex_away.position);
        }
    }

    // 新ポリゴン候補の情報をもとに，まとめられるポリゴンをまとめて保持する
    public class FusionPolygonList {
        // マージできるポリゴン情報をリストにまとめる
        Dictionary<int, List<PolygonInfo>> fusionPolygonDB = new Dictionary<int, List<PolygonInfo>>();

        public void Add(
            int edgeDirection,
            PolygonInfo polygonInfo_subject
        ) {
            List<PolygonInfo> polygonInfoList;

            // edgeDirection が同じポリゴンをマージするリストが存在しない場合は新規作成する
            if (!fusionPolygonDB.TryGetValue(edgeDirection, out polygonInfoList)) {
                polygonInfoList = new List<PolygonInfo>();
                fusionPolygonDB.Add(edgeDirection, polygonInfoList);
            }
            bool isFused = false;

            // 同じ edgeDirection のポリゴンをマージする
            for (int i = polygonInfoList.Count - 1; i >= 0; i--) {
                PolygonInfo polygonInfo_compare = polygonInfoList[i];

                // Add しようとしているポリゴンが既存のポリゴンとマージできるか判定する
                if (polygonInfo_subject.edgeDirection == polygonInfo_compare.edgeDirection) {
                    PolygonInfo towardConnection, awayConnection;
                    // polygonInfo が上から subject に接続する場合
                    if (polygonInfo_subject.vertex_toward.vertexDomein == polygonInfo_compare.vertex_away.vertexDomein) {
                        awayConnection = polygonInfo_subject;
                        towardConnection = polygonInfo_compare;
                    }
                    // polygonInfo が下から subject に接続する場合
                    else if (polygonInfo_subject.vertex_away.vertexDomein == polygonInfo_compare.vertex_toward.vertexDomein) {
                        towardConnection = polygonInfo_subject;
                        awayConnection = polygonInfo_compare;
                    }
                    // 次のループへ
                    else {
                        continue;
                    }

                    // マージ処理のためのポインタ操作を行う
                    if ((towardConnection.terminal_right.next = awayConnection.internal_right.next) != null) {
                        towardConnection.terminal_right = awayConnection.terminal_right;
                        towardConnection.count_rightVert += awayConnection.count_rightVert - 1;
                    }
                    if ((towardConnection.terminal_left.next = awayConnection.internal_left.next) != null) {
                        towardConnection.terminal_left = awayConnection.terminal_left;
                        towardConnection.count_leftVert += awayConnection.count_leftVert - 1;
                    }

                    // マージ処理を行う
                    towardConnection.vertex_away = awayConnection.vertex_away;
                    awayConnection.vertex_toward = towardConnection.vertex_toward;

                    // isFusion が true の場合は，二つのポリゴンの間に新ポリゴンがはまって三つが一つになっており，towardConnection, awayConnection の両方がマージされているので，片方を削除する
                    if (isFused) {
                        polygonInfoList.Remove(awayConnection);
                        break;
                    }
                    polygonInfoList[i] = towardConnection;
                    polygonInfo_subject = towardConnection;
                    isFused = true;
                }
            }
            if (!isFused) {
                polygonInfoList.Add(polygonInfo_subject);
            }
        }

        // マージされた多角形ポリゴンごとに三角形ポリゴンを生成する
        public void MakeTriangles() {
            int sum = 0;
            foreach (List<PolygonInfo> list in fusionPolygonDB.Values) {
                foreach (PolygonInfo f in list) {
                    f.AddTriangleCirculation();
                    sum++;
                }
            }
        }

        public void Clear() {
            fusionPolygonDB.Clear();
        }
    }

    // 新頂点とその辺向情報をもとに連結辺を生成する
    public class JoinedVertexGroup {
        public Node<Vector3> start, end;
        public Vector3 startPos, endPos;
        public int verticesCount;
        public Vector3 sum_rightPosition;
        public JoinedVertexGroup(Node<Vector3> _left, Node<Vector3> _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos) {
            start = _left;
            end = _right;
            startPos = _startPos;
            endPos = _endPos;
            verticesCount = 1;
            sum_rightPosition = rightPos;
        }
    }

    // 切断面上の図形に沿ったポリゴンを生成する
    public class SlashSurfaceGeometry {

        Dictionary<Vector3, JoinedVertexGroup> counterclockwiseDB = new Dictionary<Vector3, JoinedVertexGroup>();
        Dictionary<Vector3, JoinedVertexGroup> clockwiseDB = new Dictionary<Vector3, JoinedVertexGroup>();

        public void Add(
            Vector3 vertex_toward_position,
            Vector3 vertex_away_position
        ) {
            // 新しい連結辺を生成する
            Node<Vector3> target = new Node<Vector3>(vertex_away_position);
            JoinedVertexGroup roop1 = null;
            bool isFound1;
            // 時計回り DB の中に自身が終点のものがある場合
            if (isFound1 = clockwiseDB.ContainsKey(vertex_toward_position)) {
                roop1 = clockwiseDB[vertex_toward_position];
                // 終端に自身を追加して，終点を terget に更新する
                roop1.end.next = target;
                roop1.end = target;
                roop1.endPos = vertex_away_position;
                // DB から roop1 を削除する
                clockwiseDB.Remove(vertex_toward_position);
            }

            JoinedVertexGroup roop2 = null;
            bool isFound2;

            // 反時計回り DB の中に自身が終点のものがある場合
            if (isFound2 = counterclockwiseDB.ContainsKey(vertex_away_position)) {
                roop2 = counterclockwiseDB[vertex_away_position];
                // 両連結辺ループが等しくなれば，連結辺図形の完成
                if (roop1 == roop2) {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += vertex_away_position;
                    return;
                }
                // 終端に自身を追加して，終点を terget に更新する
                target.next = roop2.start;
                roop2.start = target;
                roop2.startPos = vertex_toward_position;
                // DB から roop2 を削除する
                counterclockwiseDB.Remove(vertex_away_position);
            }


            if (isFound1) {
                // 両連結辺ループがつながったとき
                if (isFound2) {
                    // roop1 + target + roop2 となっているので roop1 に roop2 を結合する
                    roop1.end = roop2.end;
                    roop1.endPos = roop2.endPos;
                    roop1.verticesCount += roop2.verticesCount + 1;
                    roop1.sum_rightPosition += roop2.sum_rightPosition + vertex_away_position;
                    var key = roop2.endPos;
                    clockwiseDB[key] = roop1;
                }
                // 反時計回りに繋がったとき，時計回り DB に自身の終点を追加する
                else {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += vertex_away_position;
                    // 既に追加されている場合は追加しない
                    if (counterclockwiseDB.ContainsKey(vertex_toward_position)) {
                        return;
                    }
                    clockwiseDB.Add(vertex_away_position, roop1);
                }
            } else {
                // 時計回りに繋がったとき，反時計回り DB に自身の終点を追加する
                if (isFound2) {
                    roop2.verticesCount++;
                    roop2.sum_rightPosition += vertex_away_position;
                    // 既に追加されている場合は追加しない
                    if (counterclockwiseDB.ContainsKey(vertex_toward_position)) {
                        return;
                    }
                    counterclockwiseDB.Add(vertex_toward_position, roop2);
                }
                // どちらにも繋がらなかったとき，新しい連結辺ループを生成する
                else {
                    JoinedVertexGroup newRoop = new JoinedVertexGroup(target, target, vertex_toward_position, vertex_away_position, vertex_away_position);
                    clockwiseDB.Add(vertex_away_position, newRoop);
                    counterclockwiseDB.Add(vertex_toward_position, newRoop);
                }
            }
        }

        public void MakeCutSurface(int submesh, Transform targetTransform) {
            Vector3 scale = targetTransform.localScale;
            // ワールド座標の上方向をオブジェクト座標に変換する
            Vector3 world_Up = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.up)).normalized;
            // ワールド座標の右方向をオブジェクト座標に変換する
            Vector3 world_Right = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.right)).normalized;

            // オブジェクト空間上での UV の U軸,V軸
            Vector3 uVector, vVector;
            // U軸 は切断面の法線と Y軸 との外積
            uVector = Vector3.Cross(world_Up, localPlane.normal);
            // 切断面の法線がZ軸方向のときは uVector がゼロベクトルになるので場合分けする
            uVector = (uVector.sqrMagnitude != 0) ? uVector.normalized : world_Right;
            // V軸 は U軸 と切断平面の法線との外積
            vVector = Vector3.Cross(localPlane.normal, uVector).normalized;
            // v軸 の方向をワールド座標上方向に揃える.
            if (Vector3.Dot(vVector, world_Up) < 0) {
                vVector *= -1;
            }

            float u_min, u_max, u_range;
            float v_min, v_max, v_range;

            foreach (JoinedVertexGroup roop in counterclockwiseDB.Values) {
                {
                    u_min = u_max = Vector3.Dot(uVector, roop.startPos);
                    v_min = v_max = Vector3.Dot(vVector, roop.startPos);
                    Node<Vector3> polygonInfo_subject = roop.start;

                    int count = 0;
                    do {
                        float u_value = Vector3.Dot(uVector, polygonInfo_subject.index);
                        u_min = Mathf.Min(u_min, u_value);
                        u_max = Mathf.Max(u_max, u_value);

                        float v_value = Vector3.Dot(vVector, polygonInfo_subject.index);
                        v_min = Mathf.Min(v_min, v_value);
                        v_max = Mathf.Max(v_max, v_value);

                        if (count > 1000) {
                            Debug.LogError("Something is wrong?");
                            break;
                        }
                        count++;
                    }
                    while ((polygonInfo_subject = polygonInfo_subject.next) != null);

                    u_range = u_max - u_min;
                    v_range = v_max - v_min;
                }

                // 連結辺のノードを手繰っていきながら，切断面の中心に頂点を追加して頂点番号を返す
                MakeVertex(roop.sum_rightPosition / roop.verticesCount, out int center_f, out int center_b);

                Node<Vector3> targetEdge = roop.start;
                // ループの始端の頂点を追加して頂点番号を返す
                MakeVertex(targetEdge.index, out int first_f, out int first_b);
                int previous_f = first_f;
                int previous_b = first_b;

                while (targetEdge.next != null) {
                    targetEdge = targetEdge.next;
                    //  外郭を一周するまで新頂点を追加して頂点番号を返す
                    MakeVertex(targetEdge.index, out int index_f, out int index_b);

                    mesh_right.AddSubIndices(
                        submesh,
                        center_f,
                        index_f,
                        previous_f
                    );
                    mesh_left.AddSubIndices(
                        submesh,
                        center_b,
                        previous_b,
                        index_b
                    );
                    previous_f = index_f;
                    previous_b = index_b;
                }
                mesh_right.AddSubIndices(
                    submesh,
                    center_f,
                    first_f,
                    previous_f
                );
                mesh_left.AddSubIndices(
                    submesh,
                    center_b,
                    previous_b,
                    first_b
                );
            }

            // 図形の中心座標と，新頂点の両側頂点 DB をもとに新しい頂点を生成する
            void MakeVertex(Vector3 vertexPos, out int index_right, out int index_left) {
                index_right = mesh_right.vertices.Count;
                index_left = mesh_left.vertices.Count;
                Vector2 uv;
                {
                    // position を UV に変換する
                    float uValue = Vector3.Dot(uVector, vertexPos);
                    float normalizedU = (uValue - u_min) / u_range;
                    float vValue = Vector3.Dot(vVector, vertexPos);
                    float normalizedV = (vValue - v_min) / v_range;

                    uv = new Vector2(normalizedU, normalizedV);
                }
                mesh_right.AddVertex(
                    vertexPos,
                    localPlane.normal,
                    uv
                );
                mesh_left.AddVertex(
                    vertexPos,
                    -localPlane.normal,
                    new Vector2(1 - uv.x, uv.y)
                );
            }
        }

        public void Clear() {
            counterclockwiseDB.Clear();
            clockwiseDB.Clear();
        }
    }

    // ノード情報を格納する
    public class Node<T> {
        public Node<T> next;
        public T index;
        public Node(T _index) {
            index = _index;
            next = null;
        }
    }

    public enum ConversionMode {
        Mode1,
        Mode2,
        Mode3
    }
}
