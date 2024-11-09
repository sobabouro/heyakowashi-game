using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.CodeDom;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.UIElements;
using System.IdentityModel.Claims;
using Unity.VisualScripting;
using System.Text.RegularExpressions;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class ActSubdivide : MonoBehaviour {

    // メッシュの情報を格納する
    public class MeshContainer {
        public List<Vector3>   vertices  = new List<Vector3>();
        public List<Vector3>   normals   = new List<Vector3>();
        public List<Vector2>   uvs       = new List<Vector2>();
        public List<List<int>> submesh   = new List<List<int>>();

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
            int       submeshDepartment,
            Vector3   face, 
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
    
    // 新ポリゴン候補の情報をもとに，まとめられるポリゴンをまとめて保持する
    public class FusionPolygonList {
        // edgeDirection が同じポリゴンをマージするリスト
        Dictionary<int, List<PolygonInfo>> fusionPolygonList = new Dictionary<int, List<PolygonInfo>>();

        public void Add(
            int edgeDirection,
            PolygonInfo polygonInfo
        ) {
            List<PolygonInfo> polygonInfoList;

            // edgeDirection が同じポリゴンをマージするリストが存在しない場合は新規作成する
            if (!fusionPolygonList.TryGetValue(edgeDirection, out polygonInfoList)) {
                polygonInfoList = new List<PolygonInfo>();
                fusionPolygonList.Add(edgeDirection, polygonInfoList);
            }
            bool isFusion = false;

            // 同じ edgeDirection のポリゴンをマージする
            for (int i = polygonInfoList.Count - 1; i >= 0; i--) {
                PolygonInfo subject = polygonInfoList[i];
                // Add しようとしているポリゴンが既存のポリゴンとマージできるか判定する
                if (polygonInfo.edgeDirection == subject.edgeDirection) {
                    PolygonInfo towardConnection, awayConnection;
                    // polygonInfo が上から subject に接続する場合
                    if (polygonInfo.vertex_toward.vertexDomein == subject.vertex_away.vertexDomein) {
                        awayConnection = subject;
                        towardConnection = polygonInfo;
                    }
                    // polygonInfo が下から subject に接続する場合
                    else if (polygonInfo.vertex_away.vertexDomein == subject.vertex_toward.vertexDomein) {
                        awayConnection = polygonInfo;
                        towardConnection = subject;
                    } 
                    else {
                        continue;
                    }

                    // マージ処理のためのポインタ操作を行う
                    if ((towardConnection.terminal_right.next = awayConnection.internal_right.next) != null) {
                        towardConnection.terminal_right = awayConnection.terminal_right;
                        towardConnection.rightVertCount += awayConnection.rightVertCount - 1;
                    }
                    if ((towardConnection.terminal_left.next = awayConnection.internal_left.next) != null) {
                        towardConnection.terminal_left = awayConnection.terminal_left;
                        towardConnection.leftVertCount += awayConnection.leftVertCount - 1;
                    }
                    // マージ処理を行う
                    towardConnection.vertex_away = awayConnection.vertex_away;
                    awayConnection.vertex_toward = towardConnection.vertex_toward;

                    // isFusion が true の場合は，二つのポリゴンの間に新ポリゴンがはまって三つが一つになっており，towardConnection, awayConnection の両方がマージされているので，片方を削除する
                    if (isFusion) {
                        polygonInfoList.Remove(awayConnection);
                        break;
                    }
                    polygonInfoList[i] = towardConnection;
                    polygonInfo = towardConnection;
                    isFusion = true;
                }
            }
            if (!isFusion) {
                polygonInfoList.Add(polygonInfo);
            }
        }

        public void MakeTrianges() {
            foreach (List<PolygonInfo> list in fusionPolygonList.Values) {
                foreach (PolygonInfo poly in list) {
                    poly.AddTriangleCirculation();
                }
            }
        }

        public void Clear() {
            fusionPolygonList.Clear();
        }
    }

    // 新ポリゴン候補の情報を格納する
    public class PolygonInfo {
        public int submeshDepartment;
        public int edgeDirection;
        public VertexInfo vertex_toward, vertex_away;
        public Node<int> internal_left, terminal_left, internal_right, terminal_right;
        public int rightVertCount, leftVertCount;

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
                internal_right = new Node<int>(_vertex_toward.indexInRightMesh);
                terminal_right = internal_right;
                internal_left = new Node<int>(_vertex_toward.indexInLeftMesh);
                terminal_left = new Node<int>(_vertex_away.indexInLeftMesh);
                internal_left.next = terminal_left;
                rightVertCount = 1;
                leftVertCount = 2;
            } else {
                internal_right = new Node<int>(_vertex_toward.indexInRightMesh);
                terminal_right = new Node<int>(_vertex_away.indexInRightMesh);
                internal_right.next = terminal_right;
                internal_left = new Node<int>(_vertex_toward.indexInLeftMesh);
                terminal_left = internal_left;
                rightVertCount = 2;
                leftVertCount = 1;
            }
        }

        public void AddTriangleCirculation() {
            (int index_r_t, int index_l_t) = vertex_toward.GetIndex();
            (int index_r_a, int index_l_a) = vertex_away.GetIndex();

            Node<int> node;
            int index_previous, count, halfCount;

            node = internal_right;
            index_previous = node.index;
            count = rightVertCount;
            halfCount = rightVertCount / 2;

            // 右側のポリゴンのうち，vertex_toward 寄りの頂点（internal から 中間地点）までを，vertex_toward との辺を構成する三角形に分割しながら追加していく
            for (int i = 0; i < halfCount; i++) {
                node = node.next;
                int index_current = node.index;
                mesh_right.AddSubIndices(
                    submeshDepartment,
                    index_current,
                    index_previous,
                    index_r_t
                );
                index_previous = index_current;
            }
            // 右側のポリゴンの頂点群の中間点と，vertex_toward, vertex_away で構成される三角形を追加する
            mesh_right.AddSubIndices(
                submeshDepartment,
                index_previous,
                index_r_t,
                index_r_a
            );
            // 右側のポリゴンのうち，vertex_away 寄りの頂点（中間地点から terminal）までを，vertex_away との辺を構成する三角形に分割しながら追加していく
            int remainCount = rightVertCount - halfCount - 1;
            for (int i = 0; i < remainCount; i++) {
                node = node.next;
                int index_current = node.index;
                mesh_right.AddSubIndices(
                    submeshDepartment,
                    index_current,
                    index_previous,
                    index_r_a
                );
                index_previous = index_current;
            }

            node = internal_left;
            index_previous = node.index;
            count = leftVertCount;
            halfCount = leftVertCount / 2;

            // 左側のポリゴンのうち，vertex_toward 寄りの頂点（internal から 中間地点）までを，vertex_toward との辺を構成する三角形に分割しながら追加していく
            for (int i = 0; i < halfCount; i++) {
                node = node.next;
                int index_current = node.index;
                mesh_left.AddSubIndices(
                    submeshDepartment,
                    index_current,
                    index_l_t,
                    index_previous
                );
                index_previous = index_current;
            }
            // 左側のポリゴンの頂点群の中間点と，vertex_toward, vertex_away で構成される三角形を追加する
            mesh_left.AddSubIndices(
                submeshDepartment,
                index_previous,
                index_l_a,
                index_l_t
            );
            // 左側のポリゴンのうち，vertex_away 寄りの頂点（中間地点から terminal）までを，vertex_away との辺を構成する三角形に分割しながら追加していく
            remainCount = leftVertCount - halfCount - 1;
            for (int i = 0; i < remainCount; i++) {
                node = node.next;
                int index_current = node.index;
                mesh_left.AddSubIndices(
                    submeshDepartment,
                    index_current,
                    index_l_a,
                    index_previous
                );
                index_previous = index_current;
            }
            slashSurfaceGeometry.Add(vertex_toward.position, vertex_away.position);
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
            ratio = _ratio;
            vertexDomein = (_rightTracker << 16) | _leftTracker;
            position = _position;
        }

        public (
            int indexInRightMesh, 
            int indexInLeftMesh
        ) GetIndex() {
            Vector3 rightNormal, leftNormal;
            Vector2 rightUV, leftUV;

            rightNormal = mesh_right.normals[indexInRightMesh];
            rightUV = mesh_right.uvs[indexInRightMesh];
            leftNormal = mesh_left.normals[indexInLeftMesh];
            leftUV = mesh_left.uvs[indexInLeftMesh];

            Vector3 newNormal = Vector3.Lerp(
                rightNormal,
                leftNormal,
                ratio
            );
            Vector2 newUV = Vector2.Lerp(
                rightUV,
                leftUV,
                ratio
            );

            int index_right, index_left;
            (int, int) pair;


            if (sandwichVertex.TryGetValue(vertexDomein, out pair)) {
                index_right = pair.Item1;
                index_left = pair.Item2;
            } 
            else {
                index_right = mesh_right.vertices.Count;
                mesh_right.vertices.Add(position);
                mesh_right.normals.Add(newNormal);
                mesh_right.uvs.Add(newUV);

                index_left = mesh_left.vertices.Count;
                mesh_left.vertices.Add(position);
                mesh_left.normals.Add(newNormal);
                mesh_left.uvs.Add(newUV);

                sandwichVertex.Add(vertexDomein, (index_right, index_left));
            }
            return (index_right, index_left);
        }
    }

    // 切断面上の図形に沿ったポリゴンを生成する
    public class SlashSurfaceGeometry {
        // RoopFragmentCollection
        Dictionary<Vector3, JoinedVertexGroup> counterclockwiseDB = new Dictionary<Vector3, JoinedVertexGroup>(); // leftPointDic
        Dictionary<Vector3, JoinedVertexGroup> clockwiseDB = new Dictionary<Vector3, JoinedVertexGroup>(); // rightPointDic

        public void Add(
            Vector3 vertex_toward_position, 
            Vector3 vertex_away_position
        ) {
            Node<Vector3> target = new Node<Vector3>(vertex_away_position);

            JoinedVertexGroup roop1 = null;
            bool isFound1;

            // 時計回り DB の中に自身が終点のものがある場合
            if (isFound1 = clockwiseDB.ContainsKey(vertex_toward_position)) {
                roop1 = clockwiseDB[vertex_toward_position];
                // 終端に自身を追加して，終点を terget に更新する
                roop1.end.next = target;
                roop1.end = target;
                roop1.endPosition = vertex_away_position;
                // DB から roop1 を削除する
                clockwiseDB.Remove(vertex_toward_position);
            }
            JoinedVertexGroup roop2 = null;
            bool isFound2;

            // 反時計回り DB の中に自身が終点のものがある場合
            if (isFound2 = counterclockwiseDB.ContainsKey(vertex_away_position)) {
                roop2 = counterclockwiseDB[vertex_away_position];
                // 両ループが等しくなれば，ループの完成
                if (roop1 == roop2) {
                    roop1.verticesCount++;
                    return;
                }
                // 終端に自身を追加して，終点を terget に更新する
                target.next = roop2.start;
                roop2.start = target;
                roop2.startPosition = vertex_toward_position;
                // DB から roop2 を削除する
                counterclockwiseDB.Remove(vertex_away_position);
            }

            if (isFound1) {
                // 両ループがつながったとき
                if (isFound2) {
                    // roop1 + target + roop2 となっているので roop1 に roop2 を結合する
                    roop1.end = roop2.end;
                    roop1.endPosition = roop2.endPosition;
                    roop1.verticesCount += roop2.verticesCount + 1;
                    clockwiseDB[roop2.endPosition] = roop1;
                }
                // 反時計回りに繋がったとき，時計回り DB に自身の終点を追加する
                else {
                    roop1.verticesCount++;
                    // 既に追加されている場合は追加しない
                    if (counterclockwiseDB.ContainsKey(vertex_toward_position)) {
                        return;
                    }
                    clockwiseDB.Add(vertex_away_position, roop1);
                }
            } 
            else {
                // 時計回りに繋がったとき，反時計回り DB に自身の終点を追加する
                if (isFound2) {
                    roop2.verticesCount++;
                    // 既に追加されている場合は追加しない
                    if (counterclockwiseDB.ContainsKey(vertex_toward_position)) {
                        return;
                    }
                    counterclockwiseDB.Add(vertex_toward_position, roop2);
                }
                // どちらにも繋がらなかったとき，新しいループを生成する
                else {
                    JoinedVertexGroup roop = new JoinedVertexGroup(
                        target, target, vertex_toward_position, vertex_away_position
                    );
                    // roop.verticesCount = 2;
                    counterclockwiseDB.Add(vertex_toward_position, roop);
                    clockwiseDB.Add(vertex_away_position, roop);
                }
            }
        }

        public void Clear() {
            counterclockwiseDB.Clear();
            clockwiseDB.Clear();
        }
    }

    // 切断面上の図形を格納する
    public class JoinedVertexGroup {
        // RooP
        public Node<Vector3> start, end;
        public Vector3 startPosition, endPosition;
        public int verticesCount;

        public JoinedVertexGroup(
            Node<Vector3> toward, Node<Vector3> away, Vector3 _startPosition, Vector3 _endPosition
        ) {
            start = toward;
            end = away;
            startPosition = _startPosition;
            endPosition = _endPosition;
        }
    }

    public class Node<T> {
        public Node<T> next;
        public T index;

        public Node(T _index) {
            index = _index;
            next = null;
        }
    }


    [SerializeField, Tooltip("切断面に適用するマテリアル")]
    private static Material surfaceMaterial;

    // メッシュ情報用の変数
    private static int[]         tracker;
    private static Vector3[]     originVertices;
    private static Vector3[]     originNormals;
    private static Vector2[]     originUVs;
    private static Mesh          originMesh;
    private static MeshContainer mesh_left  = new MeshContainer();
    private static MeshContainer mesh_right = new MeshContainer();

    // 切断面上のメッシュ情報を操作するための変数
    private static Dictionary<int, (int, int)> sandwichVertex       = new Dictionary<int, (int, int)>();
    private static FusionPolygonList           fusionPolygonList    = new FusionPolygonList();
    private static SlashSurfaceGeometry        slashSurfaceGeometry = new SlashSurfaceGeometry();

    private static List<Vector3>   newVertices                = new List<Vector3>();
    private static List<List<int>> joinedVertexGroup          = new List<List<int>>();
    private static List<Vector2>   new2DVertices              = new List<Vector2>();
    private static List<List<int>> nonConvexGeometry          = new List<List<int>>();
    private static List<List<int>> jointedMonotoneVertexGroup = new List<List<int>>();

    // メインメソッド
    public static void Subdivide(GameObject origin, Plane cutter) {

        DebugUtils.ToggleDebugMode();

        DebugUtils.PrintNumber(cutter, nameof(cutter));

        // 切断対象のオブジェクトのメッシュ情報
        mesh_left.Clear();
        mesh_right.Clear();

        sandwichVertex.Clear();
        fusionPolygonList.Clear();
        slashSurfaceGeometry.Clear();

        newVertices.Clear();
        joinedVertexGroup.Clear();
        new2DVertices.Clear();
        nonConvexGeometry.Clear();
        jointedMonotoneVertexGroup.Clear();

        originMesh = origin.GetComponent<MeshFilter>().mesh;
        originVertices = originMesh.vertices;
        originNormals = originMesh.normals;
        originUVs = originMesh.uv;

        // 切断対象のオブジェクトの各ポリゴンの左右判定用
        int rightHut = 0;
        int leftHut = 0;
        bool[] vertexTruthValues = new bool[originVertices.Length];
        int[] submeshIndicesAry;
        // mesh_origin の頂点が mesh_left, mesh_right での何番目の頂点に対応するかを格納する
        tracker = new int[originVertices.Length];

        // もとの頂点情報に左右情報を格納すると同時に，頂点情報を追加する
        for (int i = 0; i < originVertices.Length; i++) {
            vertexTruthValues[i] = cutter.GetSide(originVertices[i]);

            if (vertexTruthValues[i]) {
                mesh_right.AddVertex(
                    originVertices[i],
                    originNormals[i],
                    originUVs[i]
                );
                tracker[i] = rightHut++;
            }
            else {
                mesh_left.AddVertex(
                    originVertices[i],
                    originNormals[i],
                    originUVs[i]
                );
                tracker[i] = leftHut++;
            }
        }
        if (rightHut < 4 || leftHut < 4) {
            return;
        }

        // サブメッシュの数だけループ
        for (int submeshDepartment = 0; submeshDepartment < originMesh.subMeshCount; submeshDepartment++) {
            // このサブメッシュの頂点数を取得する
            submeshIndicesAry = originMesh.GetIndices(submeshDepartment);
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
                        cutter,
                        submeshDepartment,
                        new bool[3] { subIndexRTLF1, subIndexRTLF2, subIndexRTLF3 }, 
                        new int[3] { subIndex1, subIndex2, subIndex3 }
                    );
                }
            }
        }
        // 切断されたポリゴンの再構築を行う
        fusionPolygonList.MakeTrianges();

        Material[] mats = origin.GetComponent<MeshRenderer>().sharedMaterials;
        // もし切断対象の設定されたマテリアルの最後の要素が切断面用のマテリアルでない場合
        if (mats[mats.Length - 1].name != surfaceMaterial.name) {
            // 切断面用のマテリアルを追加する
            Material[] newMats = new Material[mats.Length + 1];
            mesh_right.submesh.Add(new List<int>());
            mesh_left.submesh.Add(new List<int>());
            // マテリアル配列に切断面用のマテリアルを追加する
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = surfaceMaterial;
            mats = newMats;
        }

        // 切断面ポリゴンの構築を行う

        // オブジェクトを生成する
        Mesh newMeshRight = new Mesh();
        newMeshRight.vertices = mesh_right.vertices.ToArray();
        newMeshRight.normals = mesh_right.normals.ToArray();
        newMeshRight.uv = mesh_right.uvs.ToArray();
        newMeshRight.subMeshCount = mesh_right.submesh.Count;
        for (int i = 0; i < mesh_right.submesh.Count; i++) {
            newMeshRight.SetIndices(mesh_right.submesh[i].ToArray(), MeshTopology.Triangles, i);
        }

        Mesh newMeshLeft = new Mesh();
        newMeshLeft.vertices = mesh_left.vertices.ToArray();
        newMeshLeft.normals = mesh_left.normals.ToArray();
        newMeshLeft.uv = mesh_left.uvs.ToArray();
        newMeshLeft.subMeshCount = mesh_left.submesh.Count;
        for (int i = 0; i < mesh_left.submesh.Count; i++) {
            newMeshLeft.SetIndices(mesh_left.submesh[i].ToArray(), MeshTopology.Triangles, i);
        }

        // 左側のメッシュは元のオブジェクトに適用する
        origin.name = "cut obj";
        origin.GetComponent<MeshFilter>().mesh = newMeshLeft;
        GameObject newObjRight = origin;
        // 右側のメッシュは新しいオブジェクトに適用する
        GameObject newObjLeft = new GameObject("cut obj", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody), typeof(ActSubdivide));
        newObjLeft.transform.position = origin.transform.position;
        newObjLeft.transform.rotation = origin.transform.rotation;
        newObjLeft.GetComponent<MeshFilter>().mesh = newMeshRight;

        newObjLeft.GetComponent<MeshRenderer>().materials = mats;
        newObjLeft.GetComponent<MeshCollider>().sharedMesh = newMeshLeft;
        newObjLeft.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation;
        newObjLeft.GetComponent<MeshCollider>().convex = true;
        newObjLeft.GetComponent<MeshCollider>().material = GetComponent<Collider>().material;
        newObjRight.GetComponent<MeshRenderer>().materials = mats;

        /*

        // 右側と左側のポリゴンがそれぞれ4つ以上ない場合は，立体を形成しないため終了する
        if (rightTriangles.Count < 4 || leftTriangles.Count < 4) {
            Debug.Log("rightTriangles.Count < 4 || leftTriangles.Count < 4");
            return;
        }

        DebugUtils.PrintListF8(newVerticesList, nameof(newVerticesList));
        DebugUtils.PrintList(vertexPairList, nameof(vertexPairList));

        // ひとつなぎの辺で形成されるすべての図形をリストアップする
        joinedVertexGroupList = GeometryUtils.GroupingForDetermineGeometry(
            vertexPairList
        );

        if (joinedVertexGroupList == null)
            return;
        DebugUtils.PrintList(joinedVertexGroupList, nameof(joinedVertexGroupList));

        // 新頂点の二次元座標変換する
        new2DVerticesArray = new Vector2[newVerticesList.Count];
        new2DVerticesArray = GeometryUtils.ConvertCoordinates3DTo2D(
            cutter,
            newVerticesList
        );

        DebugUtils.PrintArray(new2DVerticesArray, nameof(new2DVerticesArray));

        // 最も外郭となる処理図形 (内包図形の有無に関わらない) ごとにグループ化する
        nonConvexGeometryList = GeometryUtils.GroupingForSegmentNonMonotoneGeometry(
            new2DVerticesArray,
            joinedVertexGroupList
        );

        DebugUtils.PrintList(nonConvexGeometryList, nameof(nonConvexGeometryList));

        // 処理図形に対して，単調多角形分割を行う
        jointedMonotoneVertexGroupList = ComputationalGeometryAlgorithm.MakeMonotone(
            new2DVerticesArray,
            joinedVertexGroupList,
            nonConvexGeometryList
        );
        if (jointedMonotoneVertexGroupList == null)
            return;

        DebugUtils.PrintList(jointedMonotoneVertexGroupList, nameof(jointedMonotoneVertexGroupList));

        // 単調多角形を三角形分割する
        ComputationalGeometryAlgorithm.TriangulateMonotonePolygon(
            cutter.normal,
            targetVerticesLength,
            ref rightSortingHat,
            ref leftSortingHat,
            new2DVerticesArray,
            newVerticesList,
            jointedMonotoneVertexGroupList,
            albedoTexture,
            rightTriangles,
            rightVertices,
            rightUVs,
            leftTriangles,
            leftVertices,
            leftUVs
        );

        // 新しいオブジェクトを生成する
        CreateObject(
            rightVertices,
            rightTriangles,
            rightUVs
        );
        CreateObject(
            leftVertices,
            leftTriangles,
            leftUVs
        );
        Destroy(this.gameObject);

        */
    }

    // オブジェクト生成用メソッド
    private void CreateObject(
        List<Vector3> cutVertices, 
        List<int> cutTriangles, 
        List<Vector2> cutUVs
    ) {
        GameObject obj = new GameObject("cut obj", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody), typeof(ActSubdivide));
        var mesh = new Mesh();
        mesh.vertices = cutVertices.ToArray();
        mesh.triangles = cutTriangles.ToArray();
        mesh.uv = cutUVs.ToArray();
        mesh.RecalculateNormals();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshRenderer>().materials = GetComponent<MeshRenderer>().materials;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        obj.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation;
        obj.GetComponent<MeshCollider>().convex = true;
        obj.GetComponent<MeshCollider>().material = GetComponent<Collider>().material;
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        obj.transform.localScale = transform.localScale;
        obj.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
        obj.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;
        obj.GetComponent<ObjectManipulator>();
        obj.GetComponent<ActSubdivide>().surfaceMaterial = surfaceMaterial;
    }

    // 切断面上メッシュ情報の挿入
    private static void ProcessMixedTriangle(
        Plane  cutter, 
        int    submeshDepartment,
        bool[] vertexTruthValues,
        int[] subIndices
    ) {
        ( // ポリゴンの頂点情報を扱いやすいように整理する
            bool rtlf, 
            int[] sortedIndex
        ) = SortIndex(
            vertexTruthValues,
            subIndices[0],
            subIndices[1],
            subIndices[2]
        );

        /*
        float distance1 = 0.0f;
        float distance2 = 0.0f;
        float distance3 = 0.0f;
        float epsilon = 0.0001f;

        distance1 = Mathf.Abs(cutter.GetDistanceToPoint(originVertices[sortedIndex[0]]));
        distance2 = Mathf.Abs(cutter.GetDistanceToPoint(originVertices[sortedIndex[1]]));
        distance3 = Mathf.Abs(cutter.GetDistanceToPoint(originVertices[sortedIndex[2]]));

        // 「切断面上に孤独な頂点が存在する場合」と「切断面上にペア頂点の両方が存在する場合」はGetSide() で判定しきれないので，ここで処理する
        if ((rtlf && distance2 < epsilon && distance3 < epsilon) || (!rtlf && distance1 < epsilon)) {
            mesh_right.AddMesh(
                submeshDepartment,
                tracker[subIndices[0]],
                tracker[subIndices[1]],
                tracker[subIndices[2]]
            );
            return;
        } else if ((rtlf && distance1 < epsilon) || (!rtlf && distance2 < epsilon && distance3 < epsilon)) {
            mesh_left.AddMesh(
                submeshDepartment,
                tracker[subIndices[0]],
                tracker[subIndices[1]],
                tracker[subIndices[2]]
            );
            return;
        }
        */

        Ray ray1;
        Ray ray2;
        double direction1, direction2;
        double distance_lonlyToInternal, distance_lonlyToTerminal;
        Vector3 newVertexInternal, newVertexTerminal;
        float ratio_lonelyToInternal, ratio_lonelyToTerminal;

        // Ray を飛ばして，孤独な頂点からの距離比を算出する (飛ばす方向は固定する)
        if (rtlf) {
            ray1 = new Ray(originVertices[sortedIndex[0]], (originVertices[sortedIndex[1]] - originVertices[sortedIndex[0]]).normalized);
            ray2 = new Ray(originVertices[sortedIndex[0]], (originVertices[sortedIndex[2]] - originVertices[sortedIndex[0]]).normalized);
        } 
        else {
            ray1 = new Ray(originVertices[sortedIndex[1]], (originVertices[sortedIndex[0]] - originVertices[sortedIndex[1]]).normalized);
            ray2 = new Ray(originVertices[sortedIndex[2]], (originVertices[sortedIndex[0]] - originVertices[sortedIndex[2]]).normalized);
        }
        
        distance_lonlyToInternal = (originVertices[sortedIndex[0]] - originVertices[sortedIndex[1]]).sqrMagnitude;
        distance_lonlyToTerminal = (originVertices[sortedIndex[0]] - originVertices[sortedIndex[2]]).sqrMagnitude;
        
        cutter.Raycast(ray1, out float tempDirection1);
        direction1 = (double)tempDirection1;
        cutter.Raycast(ray2, out float tempDirection2);
        direction2 = (double)tempDirection2;

        newVertexInternal = ray1.GetPoint((float)direction1);
        newVertexTerminal = ray2.GetPoint((float)direction2);

        ratio_lonelyToInternal = (float)(direction1 * direction1 / distance_lonlyToInternal);
        ratio_lonelyToTerminal = (float)(direction2 * direction2 / distance_lonlyToTerminal);

        // 新しい辺の方向を int 型にシフトして保存する
        int edgeDirection;
        // 新頂点情報を生成する
        VertexInfo vertexInfoToward, vertexInfoAway;

        if (rtlf) {
            edgeDirection = ToIntFromVector3((newVertexTerminal - newVertexInternal).normalized);
            vertexInfoToward = new VertexInfo(
                tracker[sortedIndex[0]],
                tracker[sortedIndex[1]],
                ratio_lonelyToInternal,
                newVertexInternal
            );
            vertexInfoAway = new VertexInfo(
                tracker[sortedIndex[0]],
                tracker[sortedIndex[2]],
                ratio_lonelyToTerminal,
                newVertexTerminal
            );
        } 
        else {
            edgeDirection = ToIntFromVector3((newVertexInternal - newVertexTerminal).normalized);
            vertexInfoToward = new VertexInfo(
                tracker[sortedIndex[2]],
                tracker[sortedIndex[0]],
                ratio_lonelyToTerminal,
                newVertexTerminal
            );
            vertexInfoAway = new VertexInfo(
                tracker[sortedIndex[1]],
                tracker[sortedIndex[0]],
                ratio_lonelyToInternal,
                newVertexInternal
            );
        }
        // 新ポリゴン情報を生成する
        PolygonInfo polygonInfo = new PolygonInfo(
            rtlf,
            submeshDepartment,
            edgeDirection,
            vertexInfoToward,
            vertexInfoAway
        );
        // 新ポリゴンの切断辺方向が既存の新ポリゴンの切断辺方向と同じ（同一平面）であれば，マージする
        fusionPolygonList.Add(edgeDirection, polygonInfo);
    }

    // ポリゴンの頂点番号を，孤独な頂点を先頭に，表裏情報をもつ順番に並び替える
    public static (
        bool rtlf, 
        int[] sortedIndex
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
                return (rtlf, new int[] { submeshIndex3, submeshIndex1, submeshIndex2 });
            } 
            else {
                // t|f|t
                if (vertexTruthValues[2]) {
                    rtlf = false;
                    return (rtlf, new int[] { submeshIndex2, submeshIndex3, submeshIndex1 });
                } 
                // t|f|f
                else {
                    rtlf = true;
                    return (rtlf, new int[] { submeshIndex1, submeshIndex2, submeshIndex3 });
                }
            }
        } 
        else {
            if (vertexTruthValues[1]) {
                // f|t|t
                if (vertexTruthValues[2]) {
                    rtlf = false;
                    return (rtlf, new int[] { submeshIndex1, submeshIndex2, submeshIndex3 });
                }
                // f|t|f
                else {
                    rtlf = true;
                    return (rtlf, new int[] { submeshIndex2, submeshIndex3, submeshIndex1 });
                }
            }
            // f|f|t
            else {
                rtlf = true;
                return (rtlf, new int[] { submeshIndex3, submeshIndex1, submeshIndex2 });
            }
        }
    }

    // 切断平面上の頂点と，それらが構成する図形に対する処理系
    internal class GeometryUtils {

        // 新頂点リストから，ペア同士の探索を行い，頂点グループを生成する
        public static List<List<int>> GroupingForDetermineGeometry(
            List<int[]> vertexPairList
        ) {
            HashSet<int[]> remainingVertexPairList = new HashSet<int[]>(vertexPairList);
            List<List<int>> joinedVertexGroupList = new List<List<int>>();
            int errorCode = 0;
            int fleeze700_1 = 0;
            int fleeze700_2 = 0;

            while (remainingVertexPairList.Count > 0) {
                fleeze700_1++;
                if (fleeze700_1> 5000) {
                    errorCode = 1;
                    DebugUtils.PrintNumber(errorCode,nameof(errorCode));
                    return null;
                }
                List<int> geometry = new List<int>();
                // 最初のEdgeの開始点と終点を取得
                int[] currentEdge = remainingVertexPairList.First();
                int startVertex = currentEdge[0];
                int endVertex = currentEdge[1];
                // 最初のEdgeの頂点を追加し、削除する
                remainingVertexPairList.Remove(currentEdge);
                geometry.Add(startVertex);
                geometry.Add(endVertex);
                // 頂点が一周するまでループ
                while (startVertex != endVertex) {
                    fleeze700_2++;
                    if (fleeze700_2 > 5000) {
                        errorCode = 2;
                        DebugUtils.PrintList(geometry, nameof(geometry));
                        DebugUtils.PrintNumber(errorCode, nameof(errorCode));
                        return null;
                    }
                    // 残りの頂点リストから、前回の終点から始まるEdgeを探す
                    foreach (int[] edge in remainingVertexPairList) {
                        // 終点を更新、頂点グループに追加し、削除する
                        if (endVertex == edge[0]) {
                            endVertex = edge[1];
                            geometry.Add(endVertex);
                            remainingVertexPairList.Remove(edge);
                            break;
                        }
                        else if (endVertex == edge[1]) {
                            endVertex = edge[0];
                            geometry.Add(endVertex);
                            remainingVertexPairList.Remove(edge);
                            break;
                        }
                    }
                }
                joinedVertexGroupList.Add(geometry);
            }
            return joinedVertexGroupList;
        }

        // 図形同士の内外判定を巻き数法 (Winding Number Algorithm) で行い，処理図形ごとにグループ化する
        public static List<List<int>> GroupingForSegmentNonMonotoneGeometry(
            Vector2[] new2DVerticesArray, 
            List<List<int>> joinedVertexGroupList
        ) {
            int groupCount = joinedVertexGroupList.Count;
            Vector2 point = new Vector2 (0, 0);
            // 各図形の内外判定を行うための配列
            bool[][] isInsides = new bool[groupCount][];
            for (int i = 0; i < groupCount; i++) {
                isInsides[i] = new bool[groupCount];
            }
            bool[] visited = new bool[groupCount];
            // 処理図形グループリストに各図形を組み分けするためのリスト
            List<List<int>> nonConvexGeometryList = new List<List<int>>();
            // GroupingForDetermineGeometry で特定された図形の総当たり
            for (int i = 0; i < groupCount; i++) {
                for (int j = 0; j < groupCount; j++) {
                    // 自分自身は無視して，他の図形との内外判定を巻き数法で行う
                    if (i == j) continue;
                    point = new2DVerticesArray[joinedVertexGroupList[j][0]];
                    isInsides[i][j] = WindingNumberAlgorithm(new2DVerticesArray, point, joinedVertexGroupList[i]);
                }
            }
            // 図形iが他の図形を内包するしないにかかわらず，非被内包(笑)(処理図形)の場合は，内包図形とともにリストに追加する
            for (int i = 0; i < groupCount; i++) {
                if (visited[i]) continue;
                List<int> group = new List<int>();
                FindOutermostGeometry(isInsides, i, group, visited);
                nonConvexGeometryList.Add(group);
            }
            return nonConvexGeometryList;
        }

        // Winding Number Algorithm の実装
        private static bool WindingNumberAlgorithm(
            Vector2[] new2DVerticesArray, 
            Vector2 point, 
            List<int> toCompareGeometry
        ) {
            // 外郭の辺リストが右回りであることを前提とする
            // 辺の右側が図形の内部になる
            int windingNumber = 0;
            int vertexQuantity = toCompareGeometry.Count;
            for (int i = 0; i < vertexQuantity - 1; i++) {
                Vector2 internalVertex = new2DVerticesArray[toCompareGeometry[i]];
                Vector2 terminalVertex = new2DVerticesArray[toCompareGeometry[i + 1]];

                if (internalVertex.y <= point.y) {
                    // 辺の始点が点よりも下・辺の終点が点よりも上・辺の※右側に点がある場合
                    if (terminalVertex.y > point.y && MathUtils.IsRight(internalVertex, terminalVertex, point)) {
                        windingNumber--;
                    }
                }
                else {
                    // 辺の始点が点よりも上・辺の終点が点よりも下・辺の※左側に点がある場合
                    if (terminalVertex.y <= point.y && MathUtils.IsLeft(internalVertex, terminalVertex, point)) {
                        windingNumber++;
                    }
                }
            }
            // 0 でない場合は内部にある => true
            return windingNumber != 0;
        }

        // GroupingForSegmentNonMonotoneGeometry() の処理図形をグルーピングする補助関数
        private static void FindOutermostGeometry (
            bool[][] isInsides, 
            int index, 
            List<int> group, 
            bool[] visited
        ) {
            // すでにグルーピングした図形は無視する
            if (visited[index]) return;

            visited[index] = true;
            group.Add(index);

            for (int i = 0; i < isInsides.Length; i++) {
                // 図形 i が index に内包されている場合
                if (isInsides[index][i]) {
                    FindOutermostGeometry(isInsides, i, group, visited);
                }
                // 図形 index が図形 i に内包されている場合
                else if (isInsides[i][index]) {
                    group.Clear();
                    FindOutermostGeometry(isInsides, i, group, visited);
                    break;
                }
            }
        }

        // 平面上の頂点を2D座標に変換する関数
        public static Vector2[] ConvertCoordinates3DTo2D(
            Plane cutter, 
            List<Vector3> vertices
        ) {
            Vector2[] result = new Vector2[vertices.Count];
            Vector3 planeNormal = cutter.normal;
            Vector3 planePoint = planeNormal * cutter.distance;

            // 法線に垂直なベクトルuを生成
            Vector3 u = Vector3.Cross(planeNormal, Vector3.up).normalized;
            if (u.magnitude < 0.001f) {
                u = Vector3.Cross(planeNormal, Vector3.right).normalized;
            }
            // ベクトルuに垂直なベクトルvを生成
            Vector3 v = Vector3.Cross(planeNormal, u);

            // u, v による座標変換
            for (int i = 0; i < vertices.Count; i++) {
                Vector3 pointOnPlane = vertices[i] - planePoint;
                float x = Vector3.Dot(pointOnPlane, u);
                float y = Vector3.Dot(pointOnPlane, v);
                result[i] = new Vector2(x, y);
            }

            return result;
        }
    }

    // 単調多角形分割と，多角形の三角形分割に関する処理系
    internal class ComputationalGeometryAlgorithm {
    /*
    * [参考文献]
    * コンピュータ・ジオメトリ (計算幾何学: アルゴリズムと応用) ：近代科学社
    * M. ドバーグ, M. ファン・クリベルド, M. オーバマーズ, O. シュワルツコップ 共著
    * 浅野 哲夫 訳
    */

    /*
    * 以下は，凸でない多角形 |P| を三角形分割するためのアルゴリズムである．
    * そのために，まずは |P| を単調多角形 (monotone polygon) 配列 |P'|に分割する．
    * いきなり |P'| に分割することは困難なので，他の図形に非被内包である，
    * 外郭図形 (内包図形の有無に無関係)を，処理図形のグループとして |P_s| にする．
    *   ※ 3DCG においての用語との混同を避けるため，以降 monotone geometry とする．
    *   ※ |P'| と |P_s| との勲に注意する．
    * ここでは，|nonConvexGeometryEdgesJagAry| を疑似的に |P'| として扱う．
    * まずは，|P| の頂点を，通常点と変曲点で以下のように分類する．
    *   [ 出発点: start, 統合点: merge, 分離点: split, 最終点: end, 通常の点: regular ]
    * そして，|P'| の頂点を時計回りに並べたものをv_0, v_1, ..., v_{n-1} とする．
    * また，|P'| の辺集合を e_0, e_1, ..., e_{n-1} とする．
    * また，|P'| の辺集合と同じ大きさの配列 |Helper| を用意し，
    * v_i の頂点種類を，すぐ右の辺にあたる helper(e_{v_i}) に格納する．
    * すぐ右の辺がない場合は，自身を終点とする辺の始点が helper(e_{v_i}) となる．
    *
    * 以下は，そのアルゴリズムの具体的な手順である．
    * 1. |P_s| の頂点の分類配列を，y 座標の降順にソートする．(sごとに)
    * 2. y 座標の降順に helper(v_i) を参照していき，以下の通りに処理を行う．
    *
    *    case: 出発点
    *    (1). とくに何もしない．
    *    case: 最終点
    *    (1). もし，helper(e_{v_i}-1) が統合点の場合，
    *         - v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *    (2). helper(e_{v_i}-1) を削除する．
    *    case: 統合点
    *    (1). もし，helper(e_{v_i}-1) が統合点の場合，
    *         - v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *    (2). helper(e_{v_i}-1) を削除する．
    *    (3). 右隣の辺 e_j を探す．
    *    (4). もし，helper(e_j}) が統合点の場合，
    *         - v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *    (5). helper(e_j) に v_i を設定する．
    *    case: 分離点
    *    (1). 右隣の辺 e_j を探す．
    *    (2). v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *    (3). helper(e_j) に v_i を設定する．
    *    (4). helper(e_{v_i}) を v_i に設定する．
    *    case: 通常の点
    *    (1). もし，v_i が e_{v_i}-1 の左側にある場合，以下の処理を行う．
    *         - もし，helper(e_{v_i}-1) が統合点の場合，
    *         -- v_i と helper(e_{v_i}-1) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *         -- helper(e_{v_i}-1) を削除する．
    *         -- helper(e_{v_i}) に v_i を設定する．
    *    (2). もし，v_i が e_{v_i}-1 の右側にある場合，以下の処理を行う．
    *         - 右隣の辺 e_j を探す．
    *         - もし，helper(e_j) が統合点の場合，
    *         -- v_i と helper(e_j) を結ぶ両辺を，辺集合 {E_s} に追加する．
    *         - helper(e_j) に v_i を設定する．
    *
    * 3. {E_s} ごとに，辺のグルーピングを再度行い，|P'| を生成する．
    * 4. |P'| を三角形分割する．
    *
    * このアルゴリズムで行っていること，図形を分割する際に行わなければいけない処理は，
    * 分離点と統合点による，他の頂点からの対角線の横断を防ぐことです．
    * 例えば，統合点と分離点が，y 座標で近しい距離にあるとき，
    * つまり図形的にわかりやすく判別式を設けるなら，一貫性があればどちらでも構わない，
    * すぐ隣の辺の始点から終点の間に存在するほど近しいとき，
    * その辺から他の頂点に対して対角線を引くときに，
    * 二点による別の対角線が，横断することになります．
    * なので，その閾値による存在位置の判別により他の頂点と繋ぐことで，
    * 他の頂点からの対角線の横断を防いでいます．
    *
    * 最後に，このアルゴリズムは参考文献をもとに，自己流にアレンジしたものなので，
    * 参考文献とは重点を置いている部分が異なります．
    * 図形探索アルゴリズムに最適はあるかもしれないけど正解はないよね？
    * 参考文献にだっていやちょっと待て，と，そういう部分もあるしね．
    */

        // 新頂点の種類を格納する配列
        // private static string[] vertexType;
        // 処理図形グループ n に属する辺リスト
        // private static List<int[]> monotoneEdgeList;
        // 処理図形グループを単調多角形分割するための対角線リスト
        // private static List<int[]> monotoneDiagonalList;
        // private static NodeReference[] part_nonConvexGeometryNodesJagAry;
        // 各辺のヘルパー配列
        // private static RefInt[] helper;

        // 処理図形グループの数だけ，単調多角形分割を行う
        public static List<List<int>> MakeMonotone(
            Vector2[] new2DVerticesArray, 
            List<List<int>> joinedVertexGroupList, 
            List<List<int>> nonConvexGeometryList
        ) {
            int errorCode = 0;
            string[] vertexType = new string[new2DVerticesArray.Length];
            List<int[]> monotoneEdgeList;
            List<int[]> monotoneDiagonalList = new List<int[]>();
            List<List<int>> jointedMonotoneVertexGroupList = new List<List<int>>();
            NodeReference[] part_nonConvexGeometryNodesJagAry;
            RefInt[] helper;

            // 処理図形グループごとに，一直線上にある余分な頂点を削除する
            

            // 新頂点を種類ごとに分類する
            vertexType = ClusteringVertexType(
                new2DVerticesArray, 
                joinedVertexGroupList
            );
            DebugUtils.PrintArray(vertexType, nameof(vertexType));

            // 処理図形グループごとに，単調多角形分割を行う
            for (int processingCount = 0; processingCount < nonConvexGeometryList.Count; processingCount++) {

                // ノード配列と辺リストを生成する
                (
                    helper,
                    part_nonConvexGeometryNodesJagAry, 
                    monotoneEdgeList
                ) = GenerateNodeReference(
                    processingCount,
                    new2DVerticesArray.Length,
                    nonConvexGeometryList, 
                    joinedVertexGroupList
                );
                // ノード配列を y 座標 (降順) でソートする 
                SortNodesByCoordinateY(
                    new2DVerticesArray, 
                    part_nonConvexGeometryNodesJagAry
                );
                // 単調多角形分割のための対角線を生成する
                GenerateDiagonal(
                    new2DVerticesArray, 
                    vertexType, 
                    helper,
                    part_nonConvexGeometryNodesJagAry,
                    monotoneEdgeList
                );
                // 対角線リストから，単調多角形に分割し，多角形の辺リストに格納する
                errorCode = AssortmentToMonotone(
                    jointedMonotoneVertexGroupList, 
                    monotoneDiagonalList, 
                    monotoneEdgeList
                );
                if (errorCode != 0) {
                    return null;
                }
            }
            return jointedMonotoneVertexGroupList;
        }

        // 処理図形グループのうちの一つのグループの頂点配列を，頂点の種類に応じて探索する
        private static void GenerateDiagonal(
            Vector2[] new2DVerticesArray, 
            string[] vertexType, 
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry, 
            List<int[]> monotoneEdgeList
        ) {
            // y 座標の降順に頂点配列を探索する．
            for (int i = 0; i < part_nonConvexGeometryNodesJagAry.Length; i++) {
                // 探索対象の頂点の種類を取得する
                string targetVertexType = new string(vertexType[part_nonConvexGeometryNodesJagAry[i].CurrentVertex.Value]);
                // 出発点の場合は，特に何もしない
                // 最終点の場合
                if (targetVertexType == "end") {
                    HandleEndVertex(
                        i, 
                        vertexType, 
                        helper,
                        part_nonConvexGeometryNodesJagAry, 
                        monotoneEdgeList
                    );
                }
                // 統合点の場合
                else if (targetVertexType == "merge") {
                    HandleMergeVertex(
                        i, 
                        new2DVerticesArray, 
                        vertexType, 
                        helper,
                        part_nonConvexGeometryNodesJagAry, 
                        monotoneEdgeList
                    );
                }
                // 分離点の場合
                else if (targetVertexType == "split") {
                    HandleSplitVertex(
                        i, 
                        new2DVerticesArray, 
                        vertexType, 
                        helper,
                        part_nonConvexGeometryNodesJagAry, 
                        monotoneEdgeList
                    );
                }
                // 通常の点の場合
                else if (targetVertexType == "regular") {
                    HandleRegularVertex(
                        i, 
                        new2DVerticesArray, 
                        vertexType, 
                        helper,
                        part_nonConvexGeometryNodesJagAry, 
                        monotoneEdgeList
                    );
                }
            }
        }

        // 処理図形ごとの頂点ペアのリストを生成する
        private static (
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry,
            List<int[]> monotoneEdgeList
        ) GenerateNodeReference(
            int targetGeometry, 
            int new2DVerticesArrayLength,
            List<List<int>> nonConvexGeometryList, 
            List<List<int>> joinedVertexGroupList
        ) {
            int total = 0;
            int index = 0;
            //グループに必要な総頂点数・エッジ数を計算する
            for (int i = 0; i < nonConvexGeometryList[targetGeometry].Count; i++) {
                total += joinedVertexGroupList[nonConvexGeometryList[targetGeometry][i]].Count - 1;
            }
            // グループの辺リストを初期化する
            List<int[]> monotoneEdgeList = new List<int[]>();
            // グループのノード配列を初期化する
            //RefInt[][] part_nonConvexGeometryNodesJagAry = new RefInt[total][];
            NodeReference[] part_nonConvexGeometryNodesJagAry = new NodeReference[total];
            // ヘルパー配列を初期化する
            RefInt[] helper = new RefInt[new2DVerticesArrayLength];

            // ヘルパー配列に参照を格納する
            for (int i = 0; i < nonConvexGeometryList[targetGeometry].Count; i++) {
                List<int> vertexIndices = joinedVertexGroupList[nonConvexGeometryList[targetGeometry][i]];
                for (int j = 0; j < vertexIndices.Count - 1; j++) {
                    helper[index] = new RefInt(vertexIndices[j]);
                    index++;
                }
            }
            index = 0;

            // ノード配列・辺リストに参照と値を格納する
            for (int i = 0; i < nonConvexGeometryList[targetGeometry].Count; i++) {
                List<int> vertexIndices = joinedVertexGroupList[nonConvexGeometryList[targetGeometry][i]];
                for (int j = 0; j < vertexIndices.Count - 1; j++) {

                    part_nonConvexGeometryNodesJagAry[index] = new NodeReference(
                        // ひとつ前の頂点の参照
                        new RefInt(vertexIndices[j == 0 ? vertexIndices.Count - 2 : j - 1]),
                        // 現在の頂点の参照
                        new RefInt(vertexIndices[j]),
                        // ひとつ前の頂点のヘルパ参照
                        new Pointer<RefInt>(helper[j == 0 ? index + vertexIndices.Count - 2 : j - 1]),
                        // 現在の頂点のヘルパ参照
                        new Pointer<RefInt>(helper[j])
                    );
                    // 辺リストに頂点を格納する
                    monotoneEdgeList.Add(new int[2] { vertexIndices[j], vertexIndices[j + 1] });
                    index++;
                }
            }
            return (helper, part_nonConvexGeometryNodesJagAry, monotoneEdgeList);
        }

        // 頂点の種類を判別して，各頂点にラベルを付与する
        private static string[] ClusteringVertexType(
            Vector2[] new2DVerticesArray, 
            List<List<int>> joinedVertexGroupList
        ) {
            // 頂点の種類を格納する配列を新頂点の数と同じ大きさで用意する
            string[] vertexType = new string[new2DVerticesArray.Length];
            // GroupingForDetermineGeometry() で特定された図形ごとに頂点ラベル処理を行う
            for (int i = 0; i < joinedVertexGroupList.Count; i++) {
                for (int j = 0; j < joinedVertexGroupList[i].Count - 1; j++) {
                    Vector2 internalVertex = new2DVerticesArray[joinedVertexGroupList[i][j]];
                    Vector2 terminalVertex = new2DVerticesArray[joinedVertexGroupList[i][j + 1]];
                    Vector2 point = j == 0 ? new2DVerticesArray[joinedVertexGroupList[i][joinedVertexGroupList[j].Count - 2]] : new2DVerticesArray[joinedVertexGroupList[i][j - 1]];
                    // y座標が前後の頂点と比較して対象の点が大きいとき
                    if (internalVertex.y >= point.y && internalVertex.y > terminalVertex.y) {
                        // 部分最大の場合: 出発点
                        if (MathUtils.IsRight(internalVertex, terminalVertex, point)) {
                            vertexType[joinedVertexGroupList[i][j]] = "start";
                        }
                        // 部分極大の場合: 分離点
                        else {
                            vertexType[joinedVertexGroupList[i][j]] = "split";
                        }
                    }
                    // y座標が前後の頂点と比較して対象の点が小さいとき
                    else if (internalVertex.y <= point.y && internalVertex.y < terminalVertex.y) {
                        // 部分最小の場合: 最終点
                        if (MathUtils.IsRight(internalVertex, terminalVertex, point)) {
                            vertexType[joinedVertexGroupList[i][j]] = "end";
                        }
                        // 部分極小の場合: 統合点
                        else {
                            vertexType[joinedVertexGroupList[i][j]] = "merge";
                        }
                    }
                    // それ以外の場合: 通常の点
                    else {
                        vertexType[joinedVertexGroupList[i][j]] = "regular";
                    }
                }
            }
            return vertexType;
        }

        // 処理図形グループ頂点リストを y 座標の降順にソートする
        private static void SortNodesByCoordinateY(
            Vector2[] new2DVerticesArray,
            NodeReference[] part_nonConvexGeometryNodesJagAry
        ) {
            // 配列を対応する頂点の y座標 > x座標 の優先度で降順にソート
            Array.Sort(part_nonConvexGeometryNodesJagAry, (a, b) => {
                // y 座標を比較（降順）
                int compareY = new2DVerticesArray[b.CurrentVertex.Value].y.CompareTo(new2DVerticesArray[a.CurrentVertex.Value].y);
                if (compareY != 0) {
                    return compareY;
                }
                // y 座標が等しければ x 座標を比較（降順）
                return new2DVerticesArray[b.CurrentVertex.Value].x.CompareTo(new2DVerticesArray[a.CurrentVertex.Value].x);
            });
        }

        // 対象の頂点が最終点である場合の処理
        private static void HandleEndVertex(
            int targetNode, 
            string[] vertexType, 
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry, 
            List<int[]> monotoneDiagonalList
        ) {
            // helper[e_{v_i}-1] のインデックスを取得
            int target_previousHelperIndex = part_nonConvexGeometryNodesJagAry[targetNode].PreviousHelper.Value.Value;

            // もし，helper(e_{v_i}-1) が統合点の場合，
            if (vertexType[target_previousHelperIndex] == "merge") {
                // 現在の頂点と直前の頂点を取得
                int target_currentVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].CurrentVertex.Value;

                // v_i と helper(e_{v_i}-1) を結ぶ対角線を，辺集合 {E_s} に追加する
                monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, target_previousHelperIndex });
                monotoneDiagonalList.Add(new int[2] { target_previousHelperIndex, target_currentVertexIndex });
            }
        }

        // 対象の頂点が統合点である場合の処理
        private static void HandleMergeVertex(
            int targetNode, 
            Vector2[] new2DVerticesArray, 
            string[] vertexType, 
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry, 
            List<int[]> monotoneDiagonalList
        ) {
            // helper[e_{v_i}-1] のインデックスを取得
            int target_previousHelperIndex = part_nonConvexGeometryNodesJagAry[targetNode].PreviousHelper.Value.Value;

            // もし，helper(e_{v_i}-1) が統合点の場合，
            if (vertexType[target_previousHelperIndex] == "merge") {
                // 現在の頂点と直前の頂点を取得
                int target_currentVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].CurrentVertex.Value;

                // v_i と helper(e_{v_i}-1) を結ぶ対角線を，辺集合 {E_s} に追加する
                monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, target_previousHelperIndex });
                monotoneDiagonalList.Add(new int[2] { target_previousHelperIndex, target_currentVertexIndex });
            }
            // すぐ右隣の辺を探す
            for (int i = 0; i < part_nonConvexGeometryNodesJagAry.Length; i++) {

                int i_previousVertexIndex = part_nonConvexGeometryNodesJagAry[i].PreviousVertex.Value;
                int i_currentVertexIndex = part_nonConvexGeometryNodesJagAry[i].CurrentVertex.Value;
                int target_currentVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].CurrentVertex.Value;

                // 条件: 右隣の辺を探す
                if (new2DVerticesArray[i_previousVertexIndex].y > new2DVerticesArray[target_currentVertexIndex].y &&
                    new2DVerticesArray[i_currentVertexIndex].y <= new2DVerticesArray[target_currentVertexIndex].y) {

                    // もし，helper(e_j) が統合点の場合，
                    int i_currentHelperIndex = part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value.Value;

                    if (vertexType[i_currentHelperIndex] == "merge") {
                        // v_i と helper(e_j) を結ぶ対角線を，辺集合 {E_s} に追加する
                        monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, i_currentHelperIndex });
                        monotoneDiagonalList.Add(new int[2] { i_currentHelperIndex, target_currentVertexIndex });
                    }
                    // helper(e_j) に v_i を設定する
                    int helperIndex = RefIntUtils.FindIndexOfRefInt(helper, part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value);

                    helper[helperIndex].Value = target_currentVertexIndex;
                    break;
                }
            }
        }

        // 対象の頂点が分離点である場合の処理
        private static void HandleSplitVertex(
            int targetNode, 
            Vector2[] new2DVerticesArray, 
            string[] vertexType, 
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry, 
            List<int[]> monotoneDiagonalList
        ) {
            // 右隣の辺を探す
            for (int i = 0; i < part_nonConvexGeometryNodesJagAry.Length; i++) {
                int i_previousVertexIndex = part_nonConvexGeometryNodesJagAry[i].PreviousVertex.Value;
                int i_currentVertexIndex = part_nonConvexGeometryNodesJagAry[i].CurrentVertex.Value;
                int target_currentVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].CurrentVertex.Value;

                // 条件: 右隣の辺を探す
                if (new2DVerticesArray[i_previousVertexIndex].y > new2DVerticesArray[target_currentVertexIndex].y &&
                    new2DVerticesArray[i_currentVertexIndex].y <= new2DVerticesArray[target_currentVertexIndex].y) {

                    // v_i と helper(e_j) を結ぶ対角線を，辺集合 {E_s} に追加する
                    int i_currentHelperIndex = part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value.Value;
                    monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, i_currentHelperIndex });
                    monotoneDiagonalList.Add(new int[2] { i_currentHelperIndex, target_currentVertexIndex });

                    // helper(e_j) に v_i を設定する
                    int helperIndex = RefIntUtils.FindIndexOfRefInt(helper, part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value);
                    helper[helperIndex].Value = target_currentVertexIndex;
                    break;
                }
            }
        }

        // 対象の頂点が通常点である場合の処理
        private static void HandleRegularVertex(
            int targetNode, 
            Vector2[] new2DVerticesArray, 
            string[] vertexType, 
            RefInt[] helper,
            NodeReference[] part_nonConvexGeometryNodesJagAry, 
            List<int[]> monotoneDiagonalList
        ) {
            // もし，図形の内部が v_i の座標平面左側にある場合，以下の処理を行う
            int target_previousVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].PreviousVertex.Value;
            int target_currentVertexIndex = part_nonConvexGeometryNodesJagAry[targetNode].CurrentVertex.Value;

            if (new2DVerticesArray[target_currentVertexIndex].y < new2DVerticesArray[target_previousVertexIndex].y) {
                // もし，helper(e_{v_i}-1) が統合点の場合
                int target_previousHelperIndex = part_nonConvexGeometryNodesJagAry[targetNode].PreviousHelper.Value.Value;

                if (vertexType[target_previousHelperIndex] == "merge") {
                    // v_i と helper(e_{v_i}-1) を結ぶ対角線を，辺集合 {E_s} に追加する
                    monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, target_previousHelperIndex });
                    monotoneDiagonalList.Add(new int[2] { target_previousHelperIndex, target_currentVertexIndex });
                    // helper(e_j) に v_i を設定する
                    int helperIndex = RefIntUtils.FindIndexOfRefInt(helper, part_nonConvexGeometryNodesJagAry[targetNode].CurrentHelper.Value);

                    helper[helperIndex].Value = target_currentVertexIndex;
                }
            }
            // もし，図形の内部が v_i の座標平面右側にある場合，以下の処理を行う
            else {
                // 右隣の辺を探す
                for (int i = 0; i < part_nonConvexGeometryNodesJagAry.Length; i++) {
                    int i_previousVertexIndex = part_nonConvexGeometryNodesJagAry[i].PreviousVertex.Value;
                    int i_currentVertexIndex = part_nonConvexGeometryNodesJagAry[i].CurrentVertex.Value;

                    // 条件: 右隣の辺を探す
                    if (new2DVerticesArray[i_previousVertexIndex].y > new2DVerticesArray[target_currentVertexIndex].y &&
                        new2DVerticesArray[i_currentVertexIndex].y <= new2DVerticesArray[target_currentVertexIndex].y) {
                        // もし，helper(e_j) が統合点の場合，
                        int i_currentHelperIndex = part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value.Value;

                        if (vertexType[i_currentHelperIndex] == "merge") {
                            // v_i と helper(e_j) を結ぶ対角線を，辺集合 {E_s} に追加する
                            monotoneDiagonalList.Add(new int[2] { target_currentVertexIndex, i_currentHelperIndex });
                            monotoneDiagonalList.Add(new int[2] { i_currentHelperIndex, target_currentVertexIndex });
                        }
                        // helper(e_j) に v_i を設定する
                        //helper[RefIntUtils.FindIndexOfRefInt(helper, part_nonConvexGeometryNodesJagAry[i][3].Value)] = part_nonConvexGeometryNodesJagAry[targetNode][1].Value;
                        int helperIndex = RefIntUtils.FindIndexOfRefInt(helper, part_nonConvexGeometryNodesJagAry[i].CurrentHelper.Value);
                        helper[helperIndex].Value = target_currentVertexIndex;
                        break;
                    }
                }
            }
        }

        // 対角線リストと辺リストをもとに，単調多角形リストを生成する
        private static int AssortmentToMonotone(
            List<List<int>> jointedMonotoneVertexGroupList, 
            List<int[]> monotoneDiagonalList, 
            List<int[]> monotoneEdgeList
        ) {
            int fleeze1360_1 = 0;
            int fleeze1360_2 = 0;
            int errorCode = 0;

            while (monotoneEdgeList.Count > 0) {
                fleeze1360_1++;
                if (fleeze1360_1 > 2000) {
                    errorCode = 3;
                    Debug.Log("errorCode: " + errorCode);
                    return errorCode;
                }
                // 最初のDiagonalEdgeの開始点と終点を取得
                int startVertex = monotoneEdgeList[0][0];
                int endVertex = monotoneEdgeList[0][1];
                // 直近に追加した対角線の逆ベクトルを格納する
                int[] previousAddedDiagonal = new int[2];
                // ひとつの単調多角形の頂点リストを一時的に格納する
                List<int> currentGroup = new List<int> { startVertex, endVertex };
                MathUtils.SwapAndRemoveAt(monotoneEdgeList, 0);
                // 頂点が一周するまでループ
                while (startVertex != endVertex) {
                    fleeze1360_2++;
                    if (fleeze1360_2 > 2000) {
                        errorCode = 4;
                        Debug.Log("errorCode: " + errorCode);
                        return errorCode;
                    }
                    bool found = false;
                    // 辺リストから、前回の終点から始まる Edge を探す (in diagonal)
                    for (int i = 0; i < monotoneDiagonalList.Count; i++) {
                        var currentEdge = monotoneDiagonalList[i];
                        if (endVertex == monotoneDiagonalList[i][0] && !monotoneDiagonalList[i].SequenceEqual(previousAddedDiagonal)) {
                            // 終点を更新し、頂点グループに追加して削除する
                            endVertex = monotoneDiagonalList[i][1];
                            currentGroup.Add(endVertex);
                            previousAddedDiagonal = new int[] {monotoneDiagonalList[i][1], monotoneDiagonalList[i][0]};
                            MathUtils.SwapAndRemoveAt(monotoneDiagonalList, i);
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        // 辺リストから、前回の終点から始まる Edge を探す (in edge)
                        for (int i = 0; i < monotoneEdgeList.Count; i++) {
                            if (endVertex == monotoneEdgeList[i][0]) {
                                // 終点を更新し、頂点グループに追加して削除する
                                endVertex = monotoneEdgeList[i][1];
                                currentGroup.Add(endVertex);
                                MathUtils.SwapAndRemoveAt(monotoneEdgeList, i);
                                break;
                            }
                        }
                    }
                }
                jointedMonotoneVertexGroupList.Add(currentGroup);
            }
            return errorCode;
        }

        // 対角線リストと辺リストをもとに，トライアングルを左右のトライアングルリストに挿入する．ついでに UV も生成する
        public static void TriangulateMonotonePolygon(
            Vector3 normal, 
            int targetVerticesLength, 
            ref int rightOffset, 
            ref int leftOffset,
            Vector2[] new2DVerticesArray, 
            List<Vector3> newVerticesList, 
            List<List<int>> jointedMonotoneVertexGroupList, 
            Texture2D albedoTexture,
            List<int> rightTriangles, 
            List<Vector3> rightVertices,
            List<Vector2> rightUVs,
            List<int> leftTriangles,
            List<Vector3> leftVertices,
            List<Vector2> leftUVs
        ) {
            List<int[]> vertexConnection;
            Stack<int[]> stack;
            Vector2 uv1, uv2, uv3;

            int rightIndex, leftIndex, topIndex, bottomIndex;
            int overallRightIndex, overallLeftIndex, overallTopIndex, overallBottomIndex;

            float geometryWidth;
            float geometryHeight;

            int fleeze1440_1 = 0;
            int fleeze1440_2 = 0;

            // テクスチャマッピングのための，最大値と最小値の座標を持つ頂点のインデックスを取得する
            (overallRightIndex, overallLeftIndex, overallTopIndex, overallBottomIndex) = FindOverallMaxAndMinVertexIndices(
                new2DVerticesArray,
                jointedMonotoneVertexGroupList
            );
            geometryWidth = new2DVerticesArray[overallRightIndex].x - new2DVerticesArray[overallLeftIndex].x;
            geometryHeight = new2DVerticesArray[overallTopIndex].y - new2DVerticesArray[overallBottomIndex].y;

            // 単調多角形の数だけループ
            for (int i = 0; i < jointedMonotoneVertexGroupList.Count; i++) {
                jointedMonotoneVertexGroupList[i].RemoveAt(jointedMonotoneVertexGroupList[i].Count - 1);

                vertexConnection = new List<int[]>();
                stack = new Stack<int[]>();

                for (int j = 0; j < jointedMonotoneVertexGroupList[i].Count; j++) {
                    vertexConnection.Add(new int[2] {jointedMonotoneVertexGroupList[i][j], 0});
                }
                // 最大値と最小値の座標を持つ頂点のインデックスを取得する
                (rightIndex, leftIndex, topIndex, bottomIndex) = FindMaxAndMinVertexIndices(
                    new2DVerticesArray, 
                    jointedMonotoneVertexGroupList[i]
                );
                // 単調多角形頂点リストに top から bottom までの，境界の左右情報を加えて整列する
                PairTracingBoundaryEdge(
                    vertexConnection,
                    topIndex,
                    bottomIndex
                );
                // 一直線上にある余分な頂点を削除する
                RemoveCollinearVertices(vertexConnection, new2DVerticesArray);

                // 境界情報を持った単調多角形頂点リストを y 座標の降順にソートする
                SortMonotoneVertexByCoordinateY(
                    new2DVerticesArray, 
                    vertexConnection
                );
                // main Algorithm
                stack.Push(vertexConnection[0]);
                stack.Push(vertexConnection[1]);
                // 3番目の頂点から最後の頂点までループ
                for (int j = 2; j < vertexConnection.Count; j++) {
                    // v_j と stack.Peek() が異なる境界上の頂点同士である場合
                    if (vertexConnection[j][1] != stack.Peek()[1]) {
                        // stack のすべての頂点との間に対角線を引いた三角形を生成する
                        while (stack.Count > 0) {

                            fleeze1440_1++;
                            if (fleeze1440_1 > 1000) {
                                Debug.Log("vertexConnection " + string.Join(", ", vertexConnection.Select(obj => obj.ToString())));
                                Debug.LogError("fleeze");
                                break;
                            }

                            int[] point1 = stack.Pop();
                            int[] point2 = stack.Count == 1 ? stack.Pop() : stack.Peek();

                            // UV を計算する
                            uv1 = new Vector2(
                                (new2DVerticesArray[point1[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point1[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                            );
                            uv2 = new Vector2(
                                (new2DVerticesArray[point2[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point2[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                            );
                            uv3 = new Vector2(
                                (new2DVerticesArray[vertexConnection[j][0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[vertexConnection[j][0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                            );

                            rightTriangles.Add(rightOffset++);
                            rightTriangles.Add(rightOffset++);
                            rightTriangles.Add(rightOffset++);
                            leftTriangles.Add(leftOffset++);
                            leftTriangles.Add(leftOffset++);
                            leftTriangles.Add(leftOffset++);

                            InsertBasedOnNormal(
                                normal,
                                rightVertices,
                                rightUVs,
                                leftVertices,
                                leftUVs,
                                newVerticesList[point1[0]],
                                newVerticesList[point2[0]],
                                newVerticesList[vertexConnection[j][0]], 
                                uv1,
                                uv2,
                                uv3
                            );
                        }
                        // stack に v_j-1 と v_j を追加する
                        stack.Push(vertexConnection[j - 1]);
                        stack.Push(vertexConnection[j]);
                    }
                    // v_j と stack.Peek() が同じ境界上の頂点同士である場合
                    else {
                        // stack.Peek() までの境界線が図形の内部にある限り，繰り返し三角形を生成する
                        while (stack.Count > 0) {

                            fleeze1440_2++;
                            if (fleeze1440_2 > 1000) {
                                Debug.Log("vertexConnection " + string.Join(", ", vertexConnection.Select(obj => obj.ToString())));
                                Debug.LogError("fleeze");
                                break;
                            }

                            int[] point1 = stack.Pop();
                            int[] point2 = stack.Count == 1 ? stack.Pop() : stack.Peek();

                            // 3点が三角形を構成する場合のみ処理を行う
                            if (MathUtils.IsTriangle(new2DVerticesArray[point1[0]], new2DVerticesArray[point2[0]], new2DVerticesArray[vertexConnection[j][0]])) {

                                // 境界の右側で，v_jからの対角線が図形内部 (左) にある場合
                                if (vertexConnection[j][1] == 1 && MathUtils.IsLeft(new2DVerticesArray[vertexConnection[j][0]], new2DVerticesArray[point1[0]], new2DVerticesArray[point2[0]])) {

                                    // UV を計算する
                                    uv1 = new Vector2(
                                        (new2DVerticesArray[point1[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point1[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );
                                    uv2 = new Vector2(
                                        (new2DVerticesArray[point2[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point2[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );
                                    uv3 = new Vector2(
                                        (new2DVerticesArray[vertexConnection[j][0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[vertexConnection[j][0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );

                                    rightTriangles.Add(rightOffset++);
                                    rightTriangles.Add(rightOffset++);
                                    rightTriangles.Add(rightOffset++);
                                    leftTriangles.Add(leftOffset++);
                                    leftTriangles.Add(leftOffset++);
                                    leftTriangles.Add(leftOffset++);

                                    InsertBasedOnNormal(
                                        normal,
                                        rightVertices,
                                        rightUVs,
                                        leftVertices,
                                        leftUVs,
                                        newVerticesList[point1[0]],
                                        newVerticesList[point2[0]],
                                        newVerticesList[vertexConnection[j][0]],
                                        uv1,
                                        uv2,
                                        uv3
                                    );

                                    // stack に v_k-1 と v_j を追加する
                                    if (stack.Count == 0) {
                                        stack.Push(point2);
                                    }
                                    stack.Push(vertexConnection[j]);
                                    break;
                                }
                                // 境界の左側で，v_jからの対角線が図形内部 (右) にある場合
                                else if (vertexConnection[j][1] == -1 && MathUtils.IsRight(new2DVerticesArray[vertexConnection[j][0]], new2DVerticesArray[point1[0]], new2DVerticesArray[point2[0]])) {

                                    // UV を計算する
                                    uv1 = new Vector2(
                                        (new2DVerticesArray[point1[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point1[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );
                                    uv2 = new Vector2(
                                        (new2DVerticesArray[point2[0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[point2[0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );
                                    uv3 = new Vector2(
                                        (new2DVerticesArray[vertexConnection[j][0]].x - new2DVerticesArray[overallLeftIndex].x) / geometryWidth, (new2DVerticesArray[vertexConnection[j][0]].y - new2DVerticesArray[overallBottomIndex].y) / geometryHeight
                                    );

                                    rightTriangles.Add(rightOffset++);
                                    rightTriangles.Add(rightOffset++);
                                    rightTriangles.Add(rightOffset++);
                                    leftTriangles.Add(leftOffset++);
                                    leftTriangles.Add(leftOffset++);
                                    leftTriangles.Add(leftOffset++);

                                    InsertBasedOnNormal(
                                        normal,
                                        rightVertices,
                                        rightUVs,
                                        leftVertices,
                                        leftUVs,
                                        newVerticesList[point1[0]],
                                        newVerticesList[point2[0]],
                                        newVerticesList[vertexConnection[j][0]],
                                        uv1,
                                        uv2,
                                        uv3
                                    );

                                    // stack に v_k-1 と v_j を追加する
                                    if (stack.Count == 0) {
                                        stack.Push(point2);
                                    }
                                    stack.Push(vertexConnection[j]);
                                    break;
                                }
                                // 三角形を構成するが，図形内部に対角線が引けない場合 (反り)
                                else {
                                    // stack に v_k と v_j を追加する (初期頂点も考慮する)
                                    if(stack.Count == 0) {
                                        stack.Push(point2);
                                    }
                                    stack.Push(point1);
                                    stack.Push(vertexConnection[j]);
                                    break;
                                }
                            }
                            // 同一直線状に頂点が並ぶ場合
                            else {
                                // stack に v_k と v_j を追加する (初期頂点も考慮する)
                                if (stack.Count == 0) {
                                    stack.Push(point2);
                                }
                                stack.Push(point1);
                                stack.Push(vertexConnection[j]);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // 平面の法線と三角形の法線の向きから順番を決定して，左右に挿入する
        public static void InsertBasedOnNormal(
            Vector3 normal,
            List<Vector3> rightVertices,
            List<Vector2> rightUVs,
            List<Vector3> leftVertices,
            List<Vector2> leftUVs,
            Vector3 point_1,
            Vector3 point_2,
            Vector3 point_j, 
            Vector2 uv1,
            Vector2 uv2,
            Vector2 uv3
        ) {
            // 三角形の法線を計算
            Vector3 edge1 = point_2 - point_1;
            Vector3 edge2 = point_j - point_1;
            Vector3 triangleNormal = Vector3.Cross(edge1, edge2).normalized;
            // ドット積を計算
            float dotProduct = Vector3.Dot(normal, triangleNormal);
            // 正なら同じ向き、負なら逆向き
            if (dotProduct > 0) {
                rightVertices.AddRange(new Vector3[] {
                    point_2,
                    point_1,
                    point_j
                });
                rightUVs.AddRange(new Vector2[] {
                    uv2,
                    uv1,
                    uv3
                });
                leftVertices.AddRange(new Vector3[] {
                    point_1,
                    point_2,
                    point_j
                });
                leftUVs.AddRange(new Vector2[] {
                    uv1,
                    uv2,
                    uv3
                });
            } 
            else {
                rightVertices.AddRange(new Vector3[] {
                    point_1,
                    point_2,
                    point_j
                });
                rightUVs.AddRange(new Vector2[] {
                    uv1,
                    uv2,
                    uv3
                });
                leftVertices.AddRange(new Vector3[] {
                    point_2,
                    point_1,
                    point_j
                });
                leftUVs.AddRange(new Vector2[] {
                    uv2,
                    uv1,
                    uv3
                });
            }
        }

        // 同一直線状の頂点を削除する
        public static void RemoveCollinearVertices(List<int[]> vertexConnection, Vector2[] new2DVerticesArray) {
            int count = vertexConnection.Count;

            for (int i = 0; i < count - 2; i++) {
                // 現在の点と次の点、次の次の点を取得
                Vector2 internalVertex = new2DVerticesArray[vertexConnection[i][0]];
                Vector2 terminalVertex = new2DVerticesArray[vertexConnection[i + 1][0]];
                Vector2 nextPoint = new2DVerticesArray[vertexConnection[i + 2][0]];

                // 直線上にあるかどうかを確認
                if (Math.Abs(MathUtils.CrossProduct(internalVertex, terminalVertex, nextPoint)) < 1e-6) // 十分小さい値で比較
                {
                    // 直線上の点を削除
                    vertexConnection.RemoveAt(i + 1);
                    i--; // インデックスを戻して連続した点を確認
                    count--; // リストのサイズを更新
                }
            }
        }

        // 単調多角形リストのうち，対象の単調多角形の最大値と最小値の座標を持つ頂点のインデックスを取得する
        public static (
            int overallMaxXIndex, int overallMinXIndex,
            int overallMaxYIndex, int overallMinYIndex
        ) FindOverallMaxAndMinVertexIndices(
            Vector2[] new2DVerticesArray,
            List<List<int>> jointedMonotoneVertexGroupList
        ) {
            int overallMaxXIndex = -1;
            int overallMinXIndex = -1;
            int overallMaxYIndex = -1;
            int overallMinYIndex = -1;

            float overallMaxX = float.MinValue;
            float overallMinX = float.MaxValue;
            float overallMaxY = float.MinValue;
            float overallMinY = float.MaxValue;

            foreach (var partList in jointedMonotoneVertexGroupList) {
                var (maxXIndex, minXIndex, maxYIndex, minYIndex) = FindMaxAndMinVertexIndices(new2DVerticesArray, partList);

                float maxX = new2DVerticesArray[maxXIndex].x;
                float minX = new2DVerticesArray[minXIndex].x;
                float maxY = new2DVerticesArray[maxYIndex].y;
                float minY = new2DVerticesArray[minYIndex].y;

                if (maxX > overallMaxX || (maxX == overallMaxX && new2DVerticesArray[maxXIndex].y > new2DVerticesArray[overallMaxXIndex].y)) {
                    overallMaxX = maxX;
                    overallMaxXIndex = maxXIndex;
                }
                if (minX < overallMinX || (minX == overallMinX && new2DVerticesArray[minXIndex].y < new2DVerticesArray[overallMinXIndex].y)) {
                    overallMinX = minX;
                    overallMinXIndex = minXIndex;
                }
                if (maxY > overallMaxY || (maxY == overallMaxY && new2DVerticesArray[maxYIndex].x > new2DVerticesArray[overallMaxYIndex].x)) {
                    overallMaxY = maxY;
                    overallMaxYIndex = maxYIndex;
                }
                if (minY < overallMinY || (minY == overallMinY && new2DVerticesArray[minYIndex].x < new2DVerticesArray[overallMinYIndex].x)) {
                    overallMinY = minY;
                    overallMinYIndex = minYIndex;
                }
            }

            return (overallMaxXIndex, overallMinXIndex, overallMaxYIndex, overallMinYIndex);
        }


        // 最大値と最小値の座標を持つ頂点のインデックスを取得する
        public static (
            int maxXIndex, int minXIndex,
            int maxYIndex, int minYIndex
        ) FindMaxAndMinVertexIndices(
            Vector2[] new2DVerticesArray,
            List<int> partList
        ) {
            int maxXIndex = partList[0];
            int minXIndex = partList[0];
            int maxYIndex = partList[0];
            int minYIndex = partList[0];

            float maxX = new2DVerticesArray[maxXIndex].x;
            float minX = new2DVerticesArray[minXIndex].x;
            float maxY = new2DVerticesArray[maxYIndex].y;
            float minY = new2DVerticesArray[minYIndex].y;

            foreach (int vertexIndex in partList) {
                float currentX = new2DVerticesArray[vertexIndex].x;
                float currentY = new2DVerticesArray[vertexIndex].y;

                if (currentX > maxX || (currentX == maxX && currentY > new2DVerticesArray[maxXIndex].y)) {
                    maxX = currentX;
                    maxXIndex = vertexIndex;
                }
                if (currentX < minX || (currentX == minX && currentY < new2DVerticesArray[minXIndex].y)) {
                    minX = currentX;
                    minXIndex = vertexIndex;
                }
                if (currentY > maxY || (currentY == maxY && currentX > new2DVerticesArray[maxYIndex].x)) {
                    maxY = currentY;
                    maxYIndex = vertexIndex;
                }
                if (currentY < minY || (currentY == minY && currentX < new2DVerticesArray[minYIndex].x)) {
                    minY = currentY;
                    minYIndex = vertexIndex;
                }
            }
            return (maxXIndex, minXIndex, maxYIndex, minYIndex);
        }

        // 単調多角形リストのうち，対象の単調多角形の y 座標の降順にソートする
        private static void SortMonotoneVertexByCoordinateY(
            Vector2[] new2DVerticesArray, 
            List<int[]> vertexConnection
        ) {
            vertexConnection.Sort((a, b) => {
                // y座標を比較（降順）
                int compareY = new2DVerticesArray[b[0]].y.CompareTo(new2DVerticesArray[a[0]].y);
                if (compareY != 0) {
                    return compareY;
                }
                // y座標が等しければx座標を比較（降順）
                return new2DVerticesArray[b[0]].x.CompareTo(new2DVerticesArray[a[0]].x);
            });
        }

        // 単調多角形頂点リストを top から bottom まで，左右境界ごとに分割する
        private static void PairTracingBoundaryEdge(
            List<int[]> vertexConnection, 
            int top, 
            int bottom
        ) {
            // y 最高点の頂点を先頭とする順になるように，単調多角形頂点リストを整列する
            MathUtils.BacktrackElementsUntilTarget(
                vertexConnection, 
                top
            );
            // rightConnection: topからbottomまでの間の左側の境界を辿る
            for (int i = 1; i < vertexConnection.Count - 1; i++) {
                if (vertexConnection[i][0] == bottom) {
                    break;
                }
                // 新しい配列を作成して追加
                vertexConnection[i][1] = -1;
            }
            // leftConnection: topからbottomまでの間の右側の境界を辿る
            for (int i = vertexConnection.Count - 1; i > 0; i--) {
                if (vertexConnection[i][0] == bottom) {
                    break;
                }
                // 新しい配列を作成して追加
                vertexConnection[i][1] = 1;
            }
        }
    }

    const int filter = 0x0000FFFF; // 16ビットのマスク
    const int amp = 1 << 15;       // 拡大率（16ビット精度を確保するために適切な倍率を設定）

    public static int ToIntFromVector3(Vector3 vector) {
        // 各成分を16ビット精度でエンコード
        int cutLineX = ((int)(vector.x * amp) & filter) << 16;
        int cutLineY = ((int)(vector.y * amp) & filter) << 8;
        int cutLineZ = ((int)(vector.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }
}

// 計算に関する処理系
public class MathUtils {
    // 2つのベクトルの外積を計算する
    public static float CrossProduct(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        Vector2 v1 = terminalVertex - internalVertex;
        Vector2 v2 = point - internalVertex;
        return v1.x * v2.y - v1.y * v2.x;
    }

    // 頂点が右回りであることが前提
    public static bool IsRight(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        return CrossProduct(internalVertex, terminalVertex, point) > 0;
    }

    // 頂点が右回りであることが前提
    public static bool IsLeft(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        return CrossProduct(internalVertex, terminalVertex, point) < 0;
    }

    // 三角形を構成するかどうかを判定する
    public static bool IsTriangle(Vector2 v1, Vector2 v2, Vector2 v3) {
        // 三角形の面積を計算する
        float area = 0.5f * Math.Abs((v2.x - v1.x) * (v3.y - v1.y) - (v3.x - v1.x) * (v2.y - v1.y));
        // 面積がゼロでない場合は三角形を構成する
        return area > 0;
    }

    public static bool IsTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
        // ベクトルを計算
        Vector3 vec1 = v2 - v1;
        Vector3 vec2 = v3 - v1;

        // 外積を計算
        Vector3 crossProduct = Vector3.Cross(vec1, vec2);

        // 外積の大きさを計算し、面積を求める
        float area = 0.5f * crossProduct.magnitude;

        // 面積がゼロでない場合は三角形を構成する
        return area > 0;
    }

    // 消したい要素を末尾の要素と入れ替えて、末尾を削除する処理関数
    public static void SwapAndRemoveAt<T>(List<T> list, int indexToRemove) {
        int lastIndex = list.Count - 1;
        if (indexToRemove < lastIndex) {
            // 削除したい要素と末尾の要素を入れ替える
            list[indexToRemove] = list[lastIndex];
        }
        // 末尾の要素を削除
        list.RemoveAt(lastIndex);
    }

    // 指定要素が見つかるまでリストの要素を後ろに移動する
    public static void BacktrackElementsUntilTarget(
        List<int[]> list, 
        int target
    ) {
        int index = 0;
        int freeze2000 = 0;

        // 配列の先頭から順番に確認していき、target に到達するまで繰り返す
        while (index < list.Count && list[index][0] != target) {

            freeze2000++;
            if (freeze2000 > 1000) {
                Debug.Log("list " + string.Join(", ", list.Select(obj => obj.ToString())));
                Debug.LogError("fleeze");
                break;
            }

            // 先頭要素を取得し、リストの最後に追加
            int[] elementToMove = list[index];
            list.Add(elementToMove);
            // リストの先頭要素を削除
            list.RemoveAt(index);
        }
    }
}

// ラッパー用クラス
public class RefInt {
    public int Value {
        get; set;
    }
    public RefInt(int value) {
        Value = value;
    }
    public override bool Equals(object obj) {
        if (obj is RefInt other) {
            return this.Value == other.Value;
        }
        return false;
    }
    public override int GetHashCode() {
        return Value.GetHashCode();
    }
}

// RefInt 用のユーティリティクラス
public static class RefIntUtils {

    // RefInt 型の IndexOf() メソッド改修
    public static int FindIndexOfRefInt(RefInt[] array, RefInt value) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].Equals(value)) {
                return i;
            }
        }
        return -1;
    }
}

// pointer クラス
public class Pointer<T> {
    public T Value;
    public Pointer(T value) {
        Value = value;
    }
}

// 構造体定義
public struct VertexJudge {
    private Vector3 keyVertex;
    private string vertexLabel;
    private int vertexIndex;

    public Vector3 KeyVertex {
        get => keyVertex;
        set => keyVertex = value;
    }
    public string VertexLabel {
        get => vertexLabel;
        set => vertexLabel = value;
    }
    public int VertexIndex {
        get => vertexIndex;
        set => vertexIndex = value;
    }
    // コンストラクタを定義
    public VertexJudge(Vector3 keyVertex, string vertexLabel, int vertexIndex) {
        this.keyVertex = keyVertex;
        this.vertexLabel = vertexLabel;
        this.vertexIndex = vertexIndex;
    }
    // ToString() メソッドのオーバーライド
    public override string ToString() {
        //string vertices = string.Join(", ", pairVertices.Select(v => v.ToString()));
        string vertices = keyVertex.ToString();
        return $"PairVertices: ({vertices}), VertexLabel: {vertexLabel}, VertexIndex: {vertexIndex}";
    }
}

// 構造体定義
public struct NodeReference {
    private RefInt previousVertex;          // 直前の頂点
    private RefInt currentVertex;           // 現在の頂点
    private Pointer<RefInt> previousHelper; // 直前の辺のヘルパ
    private Pointer<RefInt> currentHelper;  // 現在の辺のヘルパ

    // PreviousVertexが変更されたらヘルパを更新
    public RefInt PreviousVertex {
        get => previousVertex;
        set {
            previousVertex = value;
            UpdateHelpers();
        }
    }

    // CurrentVertexが変更されたらヘルパを更新
    public RefInt CurrentVertex {
        get => currentVertex;
        set {
            currentVertex = value;
            UpdateHelpers();
        }
    }

    public Pointer<RefInt> PreviousHelper {
        get => previousHelper;
        set => previousHelper = value;
    }

    public Pointer<RefInt> CurrentHelper {
        get => currentHelper;
        set => currentHelper = value;
    }

    // コンストラクタを定義
    public NodeReference(RefInt previousVertex, RefInt currentVertex, Pointer<RefInt> previousHelper, Pointer<RefInt> currentHelper) {
        this.previousVertex = previousVertex;
        this.currentVertex = currentVertex;
        this.previousHelper = previousHelper;
        this.currentHelper = currentHelper;
    }

    private void UpdateHelpers() {
        // 直前の頂点でヘルパを更新
        if (previousHelper.Value != null) {
            previousHelper.Value = PreviousVertex;
        }
        // 現在の頂点でヘルパを更新
        if (currentHelper.Value != null) {
            currentHelper.Value = CurrentVertex;
        }
    }
}

// Debug用
public static class DebugUtils {
    public static bool debugMode { get; private set; } = false;

    public static void ToggleDebugMode() {
        debugMode = !debugMode;
    }
    public static void PrintNumber<T>(T value, string variableName) {
        if (debugMode) {
            string formattedValue = FormatItem(value);
            Debug.Log($"{variableName} ({typeof(T).Name}): {formattedValue}");
        }
    }
    public static void PrintVector(Vector3 vector, string variableName) {
        if (debugMode)
            Debug.Log($"{variableName} (Vector3): {vector}");
    }

    public static void PrintVector(Vector2 vector, string variableName) {
        if (debugMode)
            Debug.Log($"{variableName} (Vector2): {vector}");
    }

    public static void PrintArray(Vector2[] vectorArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", vectorArray.Select(v => v.ToString())) + "]");
        }
    }
    public static void PrintArray(Vector3[] vectorArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", vectorArray.Select(v => v.ToString())) + "]");
        }
    }
    public static void PrintArray(int[] intArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [{string.Join(", ", intArray.Select(v => v.ToString()))}]");
        }
    }
    public static void PrintArray(string[] stringArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [{string.Join(", ", stringArray.Select(v => v.ToString()))}]");
        }
    }
    public static void PrintArray(RefInt[] refIntArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [{string.Join(", ", refIntArray.Select(v => v.ToString()))}]");
        }
    }
    public static void PrintArray(RefInt[][] refIntArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(" | ", refIntArray.Select(arr => $"[{string.Join(", ", arr.Select(v => v.ToString()))}]")) + "]");
        }
    }
    public static void PrintArray(NodeReference[] nodeReferenceArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", nodeReferenceArray.Select(v => v.ToString())) + "]");
        }
    }
    public static void PrintArray(VertexJudge[] vertexJudgeArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", vertexJudgeArray.Select(v => v.ToString())) + "]");
        }
    }
    public static void PrintArrayF8(Vector2[] vectorArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", vectorArray.Select(v => v.ToString("F8"))) + "]");
        }
    }
    public static void PrintArrayF8(Vector3[] vectorArray, string arrayName) {
        if (debugMode) {
            Debug.Log($"{arrayName}: [" + string.Join(", ", vectorArray.Select(v => v.ToString("F8"))) + "]");
        }
    }
    public static void PrintList<T>(List<T> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [{string.Join(", ", list.Select(item => item.ToString()))}]");
        }
    }
    public static void PrintList<T>(List<T[]> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(arr => $"[{string.Join(", ", arr.Select(item => item.ToString()))}]")) + "]");
        }
    }
    public static void PrintList<T>(List<List<T>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(innerList => $"[{string.Join(", ", innerList.Select(item => item.ToString()))}]")) + "]");
        }
    }
    public static void PrintList<T>(List<List<T[]>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" +
                      string.Join(" | ", list.Select(innerList => "[" + string.Join(" | ", innerList.Select(arr => $"[{string.Join(", ", arr.Select(item => item.ToString()))}]")) + "]")) + "]");
        }
    }
    public static void PrintList<T>(List<List<List<T>>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(innerList => "[" + string.Join(" | ", innerList.Select(nestedList => $"[{string.Join(", ", nestedList.Select(item => item.ToString()))}]")) + "]")) + "]");
        }
    }
    public static void PrintListF8<T>(List<T> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [{string.Join(", ", list.Select(item => FormatItem(item)))}]");
        }
    }
    public static void PrintListF8<T>(List<T[]> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(arr =>
                $"[{string.Join(", ", arr.Select(item => FormatItem(item)))}]")) + "]");
        }
    }
    public static void PrintListF8<T>(List<List<T>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(innerList =>
                $"[{string.Join(", ", innerList.Select(item => FormatItem(item)))}]")) + "]");
        }
    }
    public static void PrintListF8<T>(List<List<T[]>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" +
                string.Join(" | ", list.Select(innerList => "[" +
                    string.Join(" | ", innerList.Select(arr =>
                        $"[{string.Join(", ", arr.Select(item => FormatItem(item)))}]")) + "]")) + "]");
        }
    }
    public static void PrintListF8<T>(List<List<List<T>>> list, string listName) {
        if (debugMode) {
            Debug.Log($"{listName}: [" + string.Join(" | ", list.Select(innerList => "[" +
                string.Join(" | ", innerList.Select(nestedList =>
                    $"[{string.Join(", ", nestedList.Select(item => FormatItem(item)))}]")) + "]")) + "]");
        }
    }

    // 数値型の場合は "F8" 形式を適用し、それ以外は ToString() を適用するヘルパーメソッド
    private static string FormatItem<T>(T item) {
        if (item is float or double or decimal) {
            return Convert.ToDouble(item).ToString("F8");
        } else if (item is Vector3 vec3) {
            return $"({vec3.x:F8}, {vec3.y:F8}, {vec3.z:F8})";
        } else if (item is Vector2 vec2) {
            return $"({vec2.x:F8}, {vec2.y:F8})";
        } else if (item is Plane plane) {
            return $"Normal: ({plane.normal.x:F8}, {plane.normal.y:F8}, {plane.normal.z:F8}), Distance: {plane.distance:F8}";
        } else {
            return item?.ToString() ?? "null";
        }
    }
}


