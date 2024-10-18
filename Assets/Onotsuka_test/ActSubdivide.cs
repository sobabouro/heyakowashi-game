using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.CodeDom;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class ActSubdivide : MonoBehaviour {

    [SerializeField]
    private GameObject            newGameObjectPrefab;

    [SerializeField, Tooltip("切断面に適用するマテリアル")]
    private Material              surfaceMaterial;
    private Texture2D             albedoTexture;

    // 切断対象のオブジェクトのメッシュ情報
    private static int[]          targetTriangles;
    private static Vector3[]      targetVertices;
    private static Vector3[]      targetNormals;
    private static Vector2[]      targetUVs;
    // 切断対象のオブジェクトの情報操作用
    private static int            targetVerticesLength;
    private static List<Vector3>  targetVerticesList;
    private static List<Vector3>  newVerticesList;
    private static List<int[]>    vertexPairList;
    public static List<List<int>> joinedVertexGroupList;
    public static List<List<int>> jointedMonotoneVertexGroupList;
    public static List<List<int>> nonConvexGeometryList;
    public static Vector2[]       new2DVerticesArray;
    public static string[]        vertexType;
    // 切断面左側のポリゴン情報
    public static List<int>       leftTriangles;
    public static List<Vector3>   leftNormals;
    public static List<Vector2>   leftUVs;
    // 切断面右側のポリゴン情報
    public static List<int>       rightTriangles;
    public static List<Vector3>   rightNormals;
    public static List<Vector2>   rightUVs;

    private void Start() {
        Plane cutter = new Plane(transform.right, transform.position);

        // surfaceMaterial が null でないかを確認
        if (surfaceMaterial == null) {
            Debug.LogError("surfaceMaterial is null");
            return;
        }

        // mainTexture が null でないかを確認
        if (surfaceMaterial.mainTexture == null) {
            Debug.LogError("surfaceMaterial.mainTexture is null");
            return;
        }

        // mainTexture を Texture2D にキャスト
        albedoTexture = surfaceMaterial.mainTexture as Texture2D;
        if (albedoTexture == null) {
            Debug.LogError("mainTexture is not a Texture2D");
            return;
        }

        Subdivide(cutter);
        Destroy(this.gameObject);
    }

    // メインメソッド
    public void Subdivide(Plane cutter) {
        // 切断対象のオブジェクトのメッシュ情報
        Mesh targetMesh       = this.GetComponent<MeshFilter>().mesh;
        targetTriangles       = targetMesh.triangles;
        targetVertices        = targetMesh.vertices;
        targetNormals         = targetMesh.normals;
        targetUVs             = targetMesh.uv;
        // 切断対象のオブジェクトの情報操作用
        targetVerticesLength  = targetVertices.Length;
        targetVerticesList    = new List<Vector3>(targetVertices);
        newVerticesList       = new List<Vector3>();
        vertexPairList        = new List<int[]>();
        joinedVertexGroupList = new List<List<int>>();
        // 切断面左側のポリゴン情報
        leftTriangles         = new List<int>();
        leftNormals           = new List<Vector3>();
        leftUVs               = new List<Vector2>();
        // 切断面右側のポリゴン情報
        rightTriangles        = new List<int>();
        rightNormals          = new List<Vector3>();
        rightUVs              = new List<Vector2>();

        // 切断対象のオブジェクトの各ポリゴンの左右判定用
        bool vertexTruthValue1, vertexTruthValue2, vertexTruthValue3;

        /* ****************************** */
        /* 断面の左右のポリゴンを生成する */
        /* ****************************** */

        for (int i = 0; i < targetTriangles.Length; i += 3) {
            vertexTruthValue1 = cutter.GetSide(targetVertices[targetTriangles[i]]);
            vertexTruthValue2 = cutter.GetSide(targetVertices[targetTriangles[i + 1]]);
            vertexTruthValue3 = cutter.GetSide(targetVertices[targetTriangles[i + 2]]);
            //対象の三角形ポリゴンの頂点すべてが右側にある場合
            if (vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3) {
                AddToRightSide(
                    i, 
                    targetTriangles, 
                    targetUVs, 
                    rightTriangles, 
                    rightUVs
                );
            }
            // 対象の三角形ポリゴンの頂点すべてが左側にある場合
            else if (!vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
                AddToLeftSide(
                    i, 
                    targetTriangles, 
                    targetUVs, 
                    leftTriangles, 
                    leftUVs
                );
            }
            // 対象の三角形ポリゴンの頂点が左右に分かれている場合
            else {
                ProcessMixedTriangle(
                    i, 
                    cutter,
                    vertexTruthValue1,
                    vertexTruthValue2,
                    vertexTruthValue3,
                    targetTriangles,
                    targetVertices,
                    targetUVs,
                    targetVerticesList,
                    newVerticesList,
                    vertexPairList,
                    rightTriangles,
                    leftTriangles, 
                    rightUVs,
                    leftUVs
                );
            }
        }
        /* ************************** */
        /* 断面上のポリゴンを生成する */
        /* ************************** */

        // 新頂点の二次元座標変換する
        new2DVerticesArray = new Vector2[newVerticesList.Count];
        new2DVerticesArray = GeometryUtils.ConvertCoordinates3DTo2D(
            cutter, 
            newVerticesList
        );
        // ひとつなぎの辺で形成されるすべての図形をリストアップする
        joinedVertexGroupList = GeometryUtils.GroupingForDetermineGeometry(
            vertexPairList, 
            joinedVertexGroupList
        );
        // 最も外郭となる処理図形 (内包図形の有無に関わらない) ごとにグループ化する
        nonConvexGeometryList = GeometryUtils.GroupingForSegmentNonMonotoneGeometry(
            new2DVerticesArray, 
            joinedVertexGroupList
        );
        // 処理図形に対して，単調多角形分割を行う
        jointedMonotoneVertexGroupList = new List<List<int>>();
        jointedMonotoneVertexGroupList = ComputationalGeometryAlgorithm.MakeMonotone(
            new2DVerticesArray, 
            joinedVertexGroupList, 
            nonConvexGeometryList
        );

        // albedoTexture が null でないかを確認
        if (albedoTexture == null) {
            Debug.LogError("albedoTexture is null before calling TriangulateMonotonePolygon");
            return;
        }

        // 単調多角形を三角形分割する
        ComputationalGeometryAlgorithm.TriangulateMonotonePolygon(
            targetVerticesLength, 
            new2DVerticesArray, 
            jointedMonotoneVertexGroupList, 
            albedoTexture, 
            rightTriangles, 
            rightUVs, 
            leftTriangles, 
            leftUVs
        );
        // 生成したメッシュ情報を整理する
        targetVerticesList.AddRange(newVerticesList);

        // 新しいオブジェクトを生成する
        CreateObject(
            targetVerticesList.ToArray(),
            rightTriangles.ToArray(),
            rightUVs.ToArray()
        );
        CreateRigidObject(
            targetVerticesList.ToArray(),
            leftTriangles.ToArray(),
            leftUVs.ToArray()
        );
    }

    // オブジェクト生成用メソッド
    private void CreateObject(
        Vector3[] vertices, 
        int[] triangles, 
        Vector2[] uvs
    ) {
        GameObject newObject = Instantiate(newGameObjectPrefab);
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        Mesh mesh = newObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    // リジッドボディ付きオブジェクト生成用メソッド
    private void CreateRigidObject(
        Vector3[] vertices, 
        int[] triangles, 
        Vector2[] uvs
    ) {
        GameObject newObject = Instantiate(newGameObjectPrefab);
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        Rigidbody rigid = newObject.AddComponent<Rigidbody>();
        Mesh mesh = newObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    private void AddToRightSide(
        int           triangleOffset, 
        int[]         targetTriangles, 
        Vector2[]     targetUVs, 
        List<int>     rightTriangles, 
        List<Vector2> rightUVs
    ) {
        for (int k = 0; k < 3; k++) {
            rightUVs.Add(targetUVs[targetTriangles[triangleOffset + k]]);
            rightTriangles.Add(targetTriangles[triangleOffset + k]);
        }
    }
    private void AddToLeftSide(
        int           triangleOffset, 
        int[]         targetTriangles, 
        Vector2[]     targetUVs, 
        List<int>     leftTriangles, 
        List<Vector2> leftUVs
    ) {
        for (int k = 0; k < 3; k++) {
            leftUVs.Add(targetUVs[targetTriangles[triangleOffset + k]]);
            leftTriangles.Add(targetTriangles[triangleOffset + k]);
        }
    }
    private void ProcessMixedTriangle(
        int           triangleOffset, 
        Plane         cutter, 
        bool          vertexTruthValue1, 
        bool          vertexTruthValue2, 
        bool          vertexTruthValue3, 
        int[]         targetTriangles, 
        Vector3[]     targetVertices, 
        Vector2[]     targetUVs, 
        List<Vector3> targetVerticesList, 
        List<Vector3> newVerticesList, 
        List<int[]>   vertexPairList, 
        List<int>     rightTriangles,
        List<int>     leftTriangles, 
        List<Vector2> rightUVs, 
        List<Vector2> leftUVs
    ) {
        ( // ポリゴンの頂点情報を扱いやすいように整理する
            bool rtlf, 
            int vertexIndex1, Vector3 lonelyVertex, 
            int vertexIndex2, Vector3 startPairVertex, 
            int vertexIndex3, Vector3 lastPairVertex
        ) = SegmentedPolygonsUtils.SortIndex(
            targetTriangles[triangleOffset], vertexTruthValue1, targetVertices[targetTriangles[triangleOffset]],
            targetTriangles[triangleOffset + 1], vertexTruthValue2, targetVertices[targetTriangles[triangleOffset + 1]],
            targetTriangles[triangleOffset + 2], vertexTruthValue3, targetVertices[targetTriangles[triangleOffset + 2]]
        );
        ( // 新しい頂点を生成する
            Vector3 newStartPairVertex, 
            Vector3 newLastPairVertex, 
            float ratio_LonelyAsStart, 
            float ratio_LonelyAsLast
        ) = SegmentedPolygonsUtils.GenerateNewVertex(
            cutter, rtlf, lonelyVertex, startPairVertex, lastPairVertex
        );
        ( // 新しいUV座標を生成する
            Vector2 newUV1, 
            Vector2 newUV2
        ) = SegmentedPolygonsUtils.GenerateNewUV (
            ratio_LonelyAsStart, 
            ratio_LonelyAsLast,
            targetUVs[vertexIndex1], 
            targetUVs[vertexIndex2], 
            targetUVs[vertexIndex3]
        );
        ( // 重複頂点の処理を行う (辺の始点)
            bool deltrueSV, 
            int newVertexIndexSV
        ) = SegmentedPolygonsUtils.InsertAndDeleteVertices (
            targetVerticesLength, 
            newStartPairVertex, 
            newVerticesList
        );
        if (deltrueSV == false) {
            newVerticesList.Add(newStartPairVertex);
            targetVerticesList.Add(newStartPairVertex);
        }
        ( // 重複頂点の処理を行う (辺の終点)
            bool deltrueLV, 
            int newVertexIndexLV
        ) = SegmentedPolygonsUtils.InsertAndDeleteVertices (
            targetVerticesLength, 
            newLastPairVertex, 
            newVerticesList
        );
        if (deltrueLV == false) {
            newVerticesList.Add(newLastPairVertex);
            targetVerticesList.Add(newLastPairVertex);
        }
        // のちに頂点インデックスをもとに，こいつはこいつで頂点グルーピングするので保存しておく
        int [] newVertexSet =  new int[] {newVertexIndexSV - targetVerticesLength, newVertexIndexLV - targetVerticesLength};
        vertexPairList.Add(newVertexSet);

        /* ********************************* */
        /* 孤独な頂点が無限平面の右側にある場合 */
        /* ********************************* */
        if (rtlf) {
            // 切断ポリゴン右側を生成する処理
            rightUVs.Add(targetUVs[vertexIndex1]);
            rightUVs.Add(newUV1);
            rightUVs.Add(newUV2);
            rightTriangles.Add(vertexIndex1);
            rightTriangles.Add(newVertexIndexSV);
            rightTriangles.Add(newVertexIndexLV);
            // 切断ポリゴン左側一つ目を生成する処理
            leftUVs.Add(newUV1);
            leftUVs.Add(targetUVs[vertexIndex2]);
            leftUVs.Add(targetUVs[vertexIndex3]);
            leftTriangles.Add(newVertexIndexSV);
            leftTriangles.Add(vertexIndex2);
            leftTriangles.Add(vertexIndex3);
            // 切断ポリゴン左側二つ目を生成する処理
            leftUVs.Add(targetUVs[vertexIndex3]);
            leftUVs.Add(newUV2);
            leftUVs.Add(newUV1);
            leftTriangles.Add(vertexIndex3);
            leftTriangles.Add(newVertexIndexLV);
            leftTriangles.Add(newVertexIndexSV);
        }
        /* ********************************* */
        /* 孤独な頂点が無限平面の左側にある場合 */
        /* ********************************* */
        else {
            // 切断ポリゴン左側を生成する処理
            leftUVs.Add(targetUVs[vertexIndex1]);
            leftUVs.Add(newUV1);
            leftUVs.Add(newUV2);
            leftTriangles.Add(vertexIndex1);
            leftTriangles.Add(newVertexIndexLV);
            leftTriangles.Add(newVertexIndexSV);
            // 切断ポリゴン右側一つ目を生成する処理
            rightUVs.Add(newUV1);
            rightUVs.Add(targetUVs[vertexIndex2]);
            rightUVs.Add(targetUVs[vertexIndex3]);
            rightTriangles.Add(newVertexIndexLV);
            rightTriangles.Add(vertexIndex2);
            rightTriangles.Add(vertexIndex3);
            // 切断ポリゴン右側二つ目を生成する処理
            rightUVs.Add(targetUVs[vertexIndex3]);
            rightUVs.Add(newUV2);
            rightUVs.Add(newUV1);
            rightTriangles.Add(vertexIndex3);
            rightTriangles.Add(newVertexIndexSV);
            rightTriangles.Add(newVertexIndexLV);
        }
    }

    // 分断ポリゴンに対する処理系
    internal class SegmentedPolygonsUtils {

        // ポリゴンの頂点番号を，孤独な頂点を先頭に，表裏情報をもつ順番に並び替える
        public static (
            bool rtlf, 
            int newIndex1, Vector3 lonelyVertex, 
            int newIndex2, Vector3 startPairVertex, 
            int newIndex3, Vector3 lastPairVertex
        ) SortIndex(
            int index1, bool vertexTruthValue1, Vector3 vertex1, 
            int index2, bool vertexTruthValue2, Vector3 vertex2, 
            int index3, bool vertexTruthValue3, Vector3 vertex3
        ) {
            // 孤独な頂点が無限平面の右側にある場合
            if (vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
                bool rtlf = true;
                return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
            }
            else if (!vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
                bool rtlf = true;
                return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
            }
            else if (!vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
                bool rtlf = true;
                return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
            }
            // 孤独な頂点が無限平面の左側にある頂点
            else if (vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
                bool rtlf = false;
                return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
            }
            else if (vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
                bool rtlf = false;
                return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
            }
            else { // if (!vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3)
                bool rtlf = false;
                return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
            }
        }

        // ポリゴンの切断辺の両端の頂点を，切断ポリゴンの法線・切断平面の法線とフレミングの左手の方向になるように生成する
        public static (
            Vector3 newStartPairVertex, 
            Vector3 newLastPairVertex,
            float ratio_LonelyStart,
            float ratio_LonelyLast
        ) GenerateNewVertex(
            Plane plane, 
            bool rtlf, 
            Vector3 lonelyVertex, 
            Vector3 startPairVertex, 
            Vector3 lastPairVertex
        ) {
            Ray ray1 = new Ray(lonelyVertex, startPairVertex - lonelyVertex);
            Ray ray2 = new Ray(lonelyVertex, lastPairVertex - lonelyVertex);
            float distance1 = 0.0f;
            plane.Raycast(ray1, out distance1);
            Vector3 newStartPairVertex = ray1.GetPoint(distance1);
            float distance2 = 0.0f;
            plane.Raycast(ray2, out distance2);
            Vector3 newLastPairVertex = ray2.GetPoint(distance2);

            float ratio_LonelyStart = distance1 / Vector3.Distance(lonelyVertex, startPairVertex);
            float ratio_LonelyLast = distance2 / Vector3.Distance(lonelyVertex, lastPairVertex);

            if (rtlf) {
                return (newStartPairVertex, newLastPairVertex, ratio_LonelyStart, ratio_LonelyLast);
            }
            else {
                return (newLastPairVertex, newStartPairVertex, ratio_LonelyStart, ratio_LonelyLast);
            }
        }

        // 新頂点のUV座標を生成する
        public static (
            Vector2 newUV1, 
            Vector2 newUV2
        ) GenerateNewUV(
            float ratio_LonelyAsStart, 
            float ratio_LonelyAsLast, 
            Vector2 uv1, Vector2 uv2, Vector2 uv3
        ) {
            Vector2 newUV1 = new Vector2(
                uv1.x + (uv2.x - uv1.x) * ratio_LonelyAsStart,
                uv1.y + (uv2.y - uv1.y) * ratio_LonelyAsStart
            );
            Vector2 newUV2 = new Vector2(
                uv1.x + (uv3.x - uv1.x) * ratio_LonelyAsLast,
                uv1.y + (uv3.y - uv1.y) * ratio_LonelyAsLast
            );
            return (newUV1, newUV2);
        }

        // 重複する頂点を削除する
        public static (
            bool deltrue, 
            int newVertexIndex
        ) InsertAndDeleteVertices(
            int targetVerticesLength,
            Vector3 newVertex, 
            List<Vector3> newVerticesList
        ) {
            int listCount = newVerticesList.Count;
            int newVertexIndex = listCount;
            bool deltrue = false;
            // 新頂点リストの中に重複する頂点があれば，その頂点のインデックスを返す
            for (int duplicateIndex = 0; duplicateIndex < listCount; duplicateIndex++) {
                if (newVerticesList[duplicateIndex] == newVertex) {
                    newVertexIndex = duplicateIndex;
                    deltrue = true;
                    break;
                }
            }
            return (deltrue, newVertexIndex + targetVerticesLength);
        }
    }

    // 切断平面上の頂点と，それらが構成する図形に対する処理系
    internal class GeometryUtils {

        // 新頂点リストから，ペア同士の探索を行い，頂点グループを生成する
        public static List<List<int>> GroupingForDetermineGeometry(
            List<int[]> vertexPairList,
            List<List<int>> joinedVertexGroupList
        ) {
            HashSet<int[]> remainingVertexPairList = new HashSet<int[]>(vertexPairList);

            while (remainingVertexPairList.Count > 0) {
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
                result[i] = new Vector2(x, -y);
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
        private static string[] vertexType;
        // 処理図形グループ n に属する辺リスト
        private static List<int[]> monotoneEdgeList;
        // 処理図形グループを単調多角形分割するための対角線リスト
        private static List<int[]> monotoneDiagonalList;
        private static NodeReference[] part_nonConvexGeometryNodesJagAry;
        // 各辺のヘルパー配列
        private static RefInt[] helper;

        // 処理図形グループの数だけ，単調多角形分割を行う
        public static List<List<int>> MakeMonotone(
            Vector2[] new2DVerticesArray, 
            List<List<int>> joinedVertexGroupList, 
            List<List<int>> nonConvexGeometryList
        ) {
            vertexType = new string[new2DVerticesArray.Length];
            jointedMonotoneVertexGroupList = new List<List<int>>();

            // 新頂点を種類ごとに分類する
            vertexType = ClusteringVertexType(
                new2DVerticesArray, 
                joinedVertexGroupList
            );

            // 処理図形グループごとに，単調多角形分割を行う
            for (int processingCount = 0; processingCount < nonConvexGeometryList.Count; processingCount++) {
                monotoneEdgeList = new List<int[]>();
                monotoneDiagonalList = new List<int[]>();
                // ノード配列と辺リストを生成する
                (
                    helper,
                    part_nonConvexGeometryNodesJagAry, 
                    monotoneEdgeList
                ) = GenerateNodeReference(
                    processingCount, 
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
                AssortmentToMonotone(
                    jointedMonotoneVertexGroupList, 
                    monotoneDiagonalList, 
                    monotoneEdgeList
                );
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
            RefInt[] helper = new RefInt[new2DVerticesArray.Length];

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
                    Vector2 point = j == 0 ? new2DVerticesArray[joinedVertexGroupList[i][joinedVertexGroupList[i].Count - 2]] : new2DVerticesArray[joinedVertexGroupList[i][j - 1]];
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
        private static void AssortmentToMonotone(
            List<List<int>> jointedMonotoneVertexGroupList, 
            List<int[]> monotoneDiagonalList, 
            List<int[]> monotoneEdgeList
        ) {
            while (monotoneEdgeList.Count > 0) {
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
        }

        // 対角線リストと辺リストをもとに，トライアングルを左右のトライアングルリストに挿入する．ついでに UV も生成する
        public static void TriangulateMonotonePolygon(
            int targetVerticesLength, 
            Vector2[] new2DVerticesArray, 
            List<List<int>> jointedMonotoneVertexGroupList, 
            Texture2D albedoTexture,
            List<int> rightTriangles, 
            List<Vector2> rightUVs,
            List<int> leftTriangles, 
            List<Vector2> leftUVs
        ) {
            List<int[]> vertexConnection;
            Stack<int[]> stack;
            Vector2 uv1, uv2, uv3;

            int rightIndex, leftIndex, topIndex, bottomIndex;
            int overallRightIndex, overallLeftIndex, overallTopIndex, overallBottomIndex;

            float geometryWidth;
            float geometryHeight;

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

                    // 直前のスタック操作の保存
                    //int[] previousElement, currentElement;

                    // v_j と stack.Peek() が異なる境界上の頂点同士である場合
                    if (vertexConnection[j][1] != stack.Peek()[1]) {
                        // stack のすべての頂点との間に対角線を引いた三角形を生成する
                        while (stack.Count > 0) {

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

                            // 境界の左右によってトライアングルの挿入順序が異なる
                            if (vertexConnection[j][1] == 1) {
                                rightTriangles.AddRange(new int[] { point1[0] + targetVerticesLength, point2[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                leftTriangles.AddRange(new int[] { point2[0] + targetVerticesLength, point1[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                rightUVs.AddRange(new Vector2[] { uv1, uv2, uv3 });
                                leftUVs.AddRange(new Vector2[] { uv2, uv1, uv3 });
                            } 
                            else {
                                rightTriangles.AddRange(new int[] { point2[0] + targetVerticesLength, point1[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                leftTriangles.AddRange(new int[] { point1[0] + targetVerticesLength, point2[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                rightUVs.AddRange(new Vector2[] { uv2, uv1, uv3 });
                                leftUVs.AddRange(new Vector2[] { uv1, uv2, uv3 });
                            }
                            // stack に v_j-1 と v_j を保存する
                            //currentElement = 
                            //previousElement = vertexConnection[j];
                        }
                        // stack に v_j-1 と v_j を追加する
                        stack.Push(vertexConnection[j - 1]);
                        stack.Push(vertexConnection[j]);
                    }
                    // v_j と stack.Peek() が同じ境界上の頂点同士である場合
                    else {
                        // stack.Peek() までの境界線が図形の内部にある限り，繰り返し三角形を生成する
                        while (stack.Count > 0) {

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

                                    rightTriangles.AddRange(new int[] { point1[0] + targetVerticesLength, point2[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                    leftTriangles.AddRange(new int[] { point2[0] + targetVerticesLength, point1[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                    rightUVs.AddRange(new Vector2[] { uv1, uv2, uv3 });
                                    leftUVs.AddRange(new Vector2[] { uv2, uv1, uv3 });

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

                                    rightTriangles.AddRange(new int[] { point2[0] + targetVerticesLength, point1[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                    leftTriangles.AddRange(new int[] { point1[0] + targetVerticesLength, point2[0] + targetVerticesLength, vertexConnection[j][0] + targetVerticesLength });
                                    rightUVs.AddRange(new Vector2[] { uv2, uv1, uv3 });
                                    leftUVs.AddRange(new Vector2[] { uv1, uv2, uv3 });

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
                // 最後の頂点との三角形を生成する

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
            // 配列を対応する頂点の y座標 > x座標 の優先度で降順にソート
            vertexConnection.Sort((a, b) => {
                // y 座標を比較（降順）
                int compareY = new2DVerticesArray[b[0]].y.CompareTo(new2DVerticesArray[a[0]].y);
                if (compareY != 0) {
                    return compareY;
                }
                // y 座標が等しければ x 座標を比較（降順）
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
            // rightConnection: topからbottomまでの間の右側の境界を辿る
            for (int i = 1; i < vertexConnection.Count - 1; i++) {
                if (vertexConnection[i][0] == bottom) {
                    break;
                }
                // 新しい配列を作成して追加
                vertexConnection[i][1] = 1;
            }
            // leftConnection: topからbottomまでの間の左側の境界を辿る
            for (int i = vertexConnection.Count - 1; i > 0; i--) {
                if (vertexConnection[i][0] == bottom) {
                    break;
                }
                // 新しい配列を作成して追加
                vertexConnection[i][1] = -1;
            }
        }
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
        return CrossProduct(internalVertex, terminalVertex, point) < 0;
    }

    // 頂点が右回りであることが前提
    public static bool IsLeft(
        Vector2 internalVertex, 
        Vector2 terminalVertex, 
        Vector2 point
    ) {
        return CrossProduct(internalVertex, terminalVertex, point) > 0;
    }

    // 三角形を構成するかどうかを判定する
    public static bool IsTriangle(Vector2 v1, Vector2 v2, Vector2 v3) {
        // 三角形の面積を計算する
        float area = 0.5f * Math.Abs((v2.x - v1.x) * (v3.y - v1.y) - (v3.x - v1.x) * (v2.y - v1.y));
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

        // 配列の先頭から順番に確認していき、target に到達するまで繰り返す
        while (index < list.Count && list[index][0] != target) {
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



