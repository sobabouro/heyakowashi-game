using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class ActSubdivide : MonoBehaviour {

    [SerializeField] private GameObject newGameObjectPrefab;

    private Vector3[] targetVertices;
    private Vector3[] targetNormals;
    private int[] targetTriangles;
    private Vector2[] targetUVs;

    private void Start() {
        Plane cutter = new Plane(transform.right, transform.position);
        Subdivide(cutter);
        Destroy(this.gameObject);
    }

    public void Subdivide(Plane cutter) {
        // 切断対象のオブジェクトのメッシュ情報

        Mesh targetMesh = this.GetComponent<MeshFilter>().mesh;
        targetTriangles = targetMesh.triangles;
        targetVertices  = targetMesh.vertices;
        targetNormals   = targetMesh.normals;
        targetUVs       = targetMesh.uv;

        // // 切断面左側のオブジェクトのメッシュ情報
        // var leftTriangles = new List<int>();
        // var leftVertices = new List<Vector3>();
        // var leftNormals = new List<Vector3>();
        // var leftUVs = new List<Vector2>();
        // // 切断面右側のオブジェクトのメッシュ情報
        // var rightTriangles = new List<int>();
        // var rightVertices = new List<Vector3>();
        // var rightNormals = new List<Vector3>();
        // var rightUVs = new List<Vector2>();

        // 切断対象のオブジェクトの情報操作用
        int targetVerticesLength = targetVertices.Length;
        List<int>      irrelevantTriangles  = new List<int>();
        List<Vector3>  newVerticesList      = new List<Vector3>();
        List<int[]>    vertexSetLists       = new List<int[]>();
        List<int>      newVertexSetList     = new List<int>();
        // 切断面左側のオブジェクトのメッシュ情報
        List<int>     leftTriangles = new List<int>();
        List<Vector3> leftVertices  = new List<Vector3>();
        List<Vector3> leftNormals   = new List<Vector3>();
        List<Vector2> leftUVs       = new List<Vector2>();
        // 切断面右側のオブジェクトのメッシュ情報
        List<int>     rightTriangles = new List<int>();
        List<Vector3> rightVertices  = new List<Vector3>();
        List<Vector3> rightNormals   = new List<Vector3>();
        List<Vector2> rightUVs       = new List<Vector2>();

        // 切断対象のオブジェクトの各ポリゴンの左右判定用
        List<int> numLeftVertices  = new List<int>();
        List<int> numRightVertices = new List<int>();
        bool vertexTruthValue1, vertexTruthValue2, vertexTruthValue3;

        // 既存メッシュ情報の整理
        for (int i = 0; i < targetTriangles.Length; i += 3) {
            vertexTruthValue1 = cutter.GetSide(targetVertices[targetTriangles[i]]);
            vertexTruthValue2 = cutter.GetSide(targetVertices[targetTriangles[i + 1]]);
            vertexTruthValue3 = cutter.GetSide(targetVertices[targetTriangles[i + 2]]);
            //対象の三角形ポリゴンの頂点すべてが右側にある場合
            if (vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3) {
                for (int k = 0; k < 3; k++) {
                    // もし仮動作でテクスチャの様子が何やらおかしければ，ここが原因かもしれない
                    rightUVs.Add(targetUVs[targetTriangles[i + k]]);
                    rightTriangles.Add(targetTriangles[i + k]);
                }
            }
            // 対象の三角形ポリゴンの頂点すべてが左側にある場合
            else if (!vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
                for (int k = 0; k < 3; k++) {
                    leftUVs.Add(targetUVs[targetTriangles[i + k]]);
                    leftTriangles.Add(targetTriangles[i + k]);
                }
            }
            else {
                (bool rtlf, int vertexIndex1, Vector3 lonelyVertex, int vertexIndex2, Vector3 startPairVertex, int vertexIndex3, Vector3 lastPairVertex) = SortIndex(targetTriangles[i], vertexTruthValue1, targetVertices[targetTriangles[i]], targetTriangles[i + 1], vertexTruthValue2, targetVertices[targetTriangles[i + 1]], targetTriangles[i + 2], vertexTruthValue3, targetVertices[targetTriangles[i + 2]]);
                // 新メッシュ情報の生成
                (Vector2 newRightUVs1, Vector2 newRightUVs2) = GenerateNewUV(targetUVs[vertexIndex1], targetUVs[vertexIndex2], targetUVs[vertexIndex3]);
                (Vector3 newStartPairVertex, Vector3 newLastPairVertex) = GenerateNewVertex(cutter, rtlf, lonelyVertex, startPairVertex, lastPairVertex);
                // 重複した頂点を削除する際，Vector3 で一致を探すよりも，インデックスの方が効率的なんじゃないかということ
                (bool deltrueSV, int newVertexIndexSV) = InsertAndDeleteVertices(targetVerticesLength, newStartPairVertex, newVerticesList);
                if (deltrueSV == false) {
                    newVerticesList.Add(newStartPairVertex);
                }
                (bool deltrueLV, int newVertexIndexLV) = InsertAndDeleteVertices(targetVerticesLength, newLastPairVertex, newVerticesList);
                if (deltrueLV == false) {
                    newVerticesList.Add(newLastPairVertex);
                }
                // のちに頂点インデックスをもとに頂点グルーピングするので保存しておく
                int [] vertexSet =  new int[] {newVertexIndexSV, newVertexIndexLV};
                vertexSetLists.Add(vertexSet);

                /* 孤独な頂点が無限平面の右側にある場合 */
                if (rtlf == true) {
                    // 切断ポリゴン右側を生成する処理
                    rightUVs.Add(targetUVs[vertexIndex1]);
                    rightUVs.Add(newRightUVs1);
                    rightUVs.Add(newRightUVs2);
                    rightTriangles.Add(vertexIndex1);
                    rightTriangles.Add(newVertexIndexSV);
                    rightTriangles.Add(newVertexIndexLV);
                    // 切断ポリゴン左側一つ目を生成する処理
                    rightUVs.Add(newRightUVs1);
                    rightUVs.Add(targetUVs[vertexIndex2]);
                    rightUVs.Add(targetUVs[vertexIndex3]);
                    rightTriangles.Add(newVertexIndexSV);
                    rightTriangles.Add(vertexIndex2);
                    rightTriangles.Add(vertexIndex3);
                    // 切断ポリゴン左側二つ目を生成する処理
                    leftUVs.Add(targetUVs[vertexIndex3]);
                    leftUVs.Add(newRightUVs2);
                    leftUVs.Add(newRightUVs1);
                    leftTriangles.Add(vertexIndex3);
                    leftTriangles.Add(newVertexIndexLV);
                    leftTriangles.Add(newVertexIndexSV);
                }
                /* 孤独な頂点が無限平面の左側にある場合 */
                else {
                    // 切断ポリゴン左側を生成する処理
                    leftUVs.Add(targetUVs[vertexIndex1]);
                    leftUVs.Add(newRightUVs2);
                    leftUVs.Add(newRightUVs1);
                    leftTriangles.Add(vertexIndex1);
                    leftTriangles.Add(newVertexIndexLV);
                    leftTriangles.Add(newVertexIndexSV);
                    // 切断ポリゴン右側一つ目を生成する処理
                    leftUVs.Add(newRightUVs2);
                    leftUVs.Add(targetUVs[vertexIndex2]);
                    leftUVs.Add(targetUVs[vertexIndex3]);
                    leftTriangles.Add(newVertexIndexLV);
                    leftTriangles.Add(vertexIndex2);
                    leftTriangles.Add(vertexIndex3);
                    // 切断ポリゴン右側二つ目を生成する処理
                    rightUVs.Add(targetUVs[vertexIndex3]);
                    rightUVs.Add(newRightUVs1);
                    rightUVs.Add(newRightUVs2);
                    rightTriangles.Add(vertexIndex3);
                    rightTriangles.Add(newVertexIndexSV);
                    rightTriangles.Add(newVertexIndexLV);
                }
            }
        }
        /* 断面のメッシュを生成する */
        newVertexSetList = VertexGrouping(vertexSetLists, newVertexSetList);
        // Debug
        Debug.Log(string.Join(", ", newVertexSetList.Select(obj => obj.ToString())));
    }

    // ポリゴンの頂点番号を，孤独な頂点を先頭に，表裏情報をもつ順番に並び替える
    (bool rtlf, int newIndex1, Vector3 lonelyVertex, int newIndex2, Vector3 startPairVertex, int newIndex3, Vector3 lastPairVertex) SortIndex(int index1, bool vertexTruthValue1, Vector3 vertex1, int index2, bool vertexTruthValue2, Vector3 vertex2, int index3, bool vertexTruthValue3, Vector3 vertex3) {
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
        else { // (!vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3)
            bool rtlf = false;
            return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
        }
    }

    // ポリゴンの切断辺の両端の頂点を，切断ポリゴンの法線・切断平面の法線とフレミングの左手の方向になるように生成する
    (Vector3 newStartPairVertex, Vector3 newLastPairVertex) GenerateNewVertex(Plane plane, bool rtlf, Vector3 lonelyVertex, Vector3 startPairVertex, Vector3 lastPairVertex) {
        Ray ray1 = new Ray(lonelyVertex, startPairVertex - lonelyVertex);
        Ray ray2 = new Ray(lonelyVertex, lastPairVertex - lonelyVertex);
        float distance1 = 0.0f;
        plane.Raycast(ray1, out distance1);
        Vector3 newStartPairVertex = ray1.GetPoint(distance1);
        float distance2 = 0.0f;
        plane.Raycast(ray2, out distance2);
        Vector3 newLastPairVertex = ray2.GetPoint(distance2);
        if (rtlf) {
            return (newStartPairVertex, newLastPairVertex);
        }
        else {
            return (newLastPairVertex, newStartPairVertex);
        }
    }

    // 新頂点のUV座標を生成する
    (Vector2 newUVs1, Vector2 newUVs2) GenerateNewUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
        Vector2 newUVs1 = (uv1 + uv2) / 2;
        Vector2 newUVs2 = (uv1 + uv3) / 2;
        return (newUVs1, newUVs2);
    }

    // 重複する頂点を削除する
    (bool deltrue, int newVertexIndex) InsertAndDeleteVertices(int length, Vector3 newVertex, List<Vector3> verticesList) {
        int listCount = verticesList.Count;
        int newVertexIndex = listCount;
        bool deltrue = false;
        for (int duplicateIndex = 0; duplicateIndex < listCount; duplicateIndex++) {
            if (verticesList[duplicateIndex] == newVertex) {
                newVertexIndex = duplicateIndex;
                deltrue = true;
                break;
            }
        }
        return (deltrue, newVertexIndex + length);
    }

    // 新頂点リストから，ペア同士の探索を行い，頂点グループを生成する
    private List<int> VertexGrouping(List<int[]> vertexSetLists, List<int> newVertexSetList) {
        List<int[]> vertexSetListsCopy = new List<int[]>(vertexSetLists);
        List<int[]> newVertexSetListsCopy = new List<int[]>(vertexSetLists);
        int vertexSetCount = vertexSetLists.Count;
        int removeProcessedCount = 0;
        int serchCount = 0;
        int start = vertexSetListsCopy[0][0];
        int last = vertexSetListsCopy[0][1];

        newVertexSetList.Add(start);
        newVertexSetList.Add(last);
        newVertexSetListsCopy.RemoveAt(0);
        removeProcessedCount++;
        while (start == last) {
            serchCount = 0;
            foreach (int[] vertexSet in vertexSetListsCopy) {
                if (last == vertexSet[0]) {
                    last = vertexSet[1];
                    newVertexSetList.Add(last);
                    newVertexSetListsCopy.RemoveAt(serchCount);
                    removeProcessedCount++;
                }
                serchCount++;
            }
            vertexSetListsCopy.Clear();
            vertexSetListsCopy.AddRange(newVertexSetListsCopy);
        }
        if (removeProcessedCount == vertexSetCount) {
            return newVertexSetList;
        }
        else {
            newVertexSetList.Add(-1);
            return VertexGrouping(vertexSetListsCopy, newVertexSetList);
        }
    }

    // 新しく生成した頂点・辺の情報の詳細分類
    private AssortVertexIndex DecomposeVertexSet(int length, List<int> newVertexSetList, Vector2[] verticesList2D) {
        // データをいい感じに加工する
        List<List<int>> vertexGroupList = new List<List<int>>();
        foreach (int vertexIndex in newVertexSetList) {
            if (vertexIndex == -1) {
                vertexGroupList.Add(new List<int>());
            }
            else {
                vertexGroupList[vertexGroupList.Count-1].Add(vertexIndex);
            }
        }
        AssortVertexIndex assortVertexIndex = new AssortVertexIndex();
        // ここからが本番
        foreach (List<int> vertexGroup in vertexGroupList){
            // 最初の部分
            int preVertexIndex = vertexGroup[vertexGroup.Count-1];
            int nowVertexIndex = vertexGroup[0];
            int nextVertexIndex vertexGroup[1];
            // 頂点座標の取得
            Vector2 preVertex = verticesList2D[preVertexIndex-length];
            Vector2 nowVertex = verticesList2D[nowVertexIndex-length];
            Vector2 nextVertex = verticesList2D[nextVertexIndex-length];
            // 外積
            DoubleVector2 preToNow = new DoubleVector2(nowVertex - preVertex);
            DoubleVector2 nowToNext = new DoubleVector2(nextVertex - nowVertex);
            DoubleVector2 crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
            // 判別
            if ((crossProduct < 0) && (nowVertex.y > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                assortVertexIndex.StartVertexList.Add(nowVertexIndex);
            }else if ((crossProduct < 0) && (nowVertex.y  > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                assortVertexIndex.SplitVertexList.Add(nowVertexIndex);
            }else if((crossProduct < 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                assortVertexIndex.EndVertexList.Add(nowVertexIndex);
            }else if((crossProduct > 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                assortVertexIndex.MergeVertexList.Add(nowVertexIndex);
            }
            // 2つ目以降
            for(int i = 0; i < vertexGroup.Count-1; i++){
                int preVertex = vertexGroup[i];
                int nowVertex = vertexGroup[i+1];
                int nextVertex vertexGroup[i+2];
                // 頂点座標の取得
                Vector2 preVertex = verticesList2D[preVertexIndex-length];
                Vector2 nowVertex = verticesList2D[nowVertexIndex-length];
                Vector2 nextVertex = verticesList2D[nextVertexIndex-length];
                // 外積
                DoubleVector2 preToNow = new DoubleVector2(nowVertex - preVertex);
                DoubleVector2 nowToNext = new DoubleVector2(nextVertex - nowVertex);
                DoubleVector2 crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
                // 判別
                if ((crossProduct < 0) && (nowVertex.y  > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                    assortVertexIndex.StartVertexList.Add(nowVertexIndex);
                }else if ((crossProduct < 0) && (nowVertex.y  > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                    assortVertexIndex.SplitVertexList.Add(nowVertexIndex);
                }else if((crossProduct < 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                    assortVertexIndex.EndVertexList.Add(nowVertexIndex);
                }else if((crossProduct > 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                    assortVertexIndex.MergeVertexList.Add(nowVertexIndex);
                }
            }
            // 最後の部分
            int preVertex = vertexGroup[vertexGroup.Count-2];
            int nowVertex = vertexGroup[vertexGroup.Count-1];
            int nextVertex vertexGroup[0];
            // 頂点座標の取得
            Vector2 preVertex = verticesList2D[preVertexIndex-length];
            Vector2 nowVertex = verticesList2D[nowVertexIndex-length];
            Vector2 nextVertex = verticesList2D[nextVertexIndex-length];
            // 外積
            DoubleVector2 preToNow = new DoubleVector2(nowVertex - preVertex);
            DoubleVector2 nowToNext = new DoubleVector2(nextVertex - nowVertex);
            DoubleVector2 crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
            // 判別
            if ((crossProduct < 0) && (nowVertex.y  > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                assortVertexIndex.StartVertexList.Add(nowVertexIndex);
            }else if ((crossProduct < 0) && (nowVertex.y  > preVertex.y ) && (nowVertex.y  > preVertex.y )){
                assortVertexIndex.SplitVertexList.Add(nowVertexIndex);
            }else if((crossProduct < 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                assortVertexIndex.EndVertexList.Add(nowVertexIndex);
            }else if((crossProduct > 0) && (nowVertex.y  < preVertex.y ) && (nowVertex.y  < preVertex.y )){
                assortVertexIndex.MergeVertexList.Add(nowVertexIndex);
            }
        }
        return assortVertexIndex;
    }

    // すべての頂点に対して水平方向左に隣接する辺を求める．
    private int[] SetHorizontallyAdjacentEdges(int length, List<int[]> vertexSetLists, Vector2[] verticesAry2D) {
        // すべての頂点に対応する配列を作る．
        int[] leftHorizontallyAdjacentEdges = new int[verticesAry2D.Length];
        // 各頂点からの水平方向の直線と交差する辺を探索する
        for(int i =0; i < verticesAry2D.Length; i++) {
            // 現在の見ている頂点
            Vector2 targetVertex = verticesAry2D[i];
            // 各辺について水平線に交差しているかを判定する
            foreach(int[] vertexSet in vertexSetLists) {
                // 一番近い辺のx座標
                float preCrossPointX = -Mathf.Infinity;
                // 現在見ている辺の頂点
                Vector2 startVertex = verticesAry2D[vertexSet[0]-length];
                Vector2 endVertex = verticesAry2D[vertexSet[1]-length];

                // startVertex と endVertex の間に targetVertex があるかどうか？ つまり交差してるってこと
                if((startVertex.y <= targetVertex.y && endVertex.y > targetVertex.y) || (startVertex.y > targetVertex.y && endVertex.y <= targetVertex.y) ) {
                    // 水平線と辺の交点を求める
                    float y = targetVertex.y;
                    //  x = (y - y1)(x2 - x1) / (y2 - y1) + x1;
                    float x = (y - startVertex.y)(endVertex.x - startVertex.x) / (endVertex.y - startVertex.y) + startVertex.x;
                    Vector2 crossPoint = new Vector2(x, y);
                    // それは左ですか？しかも一番近いですか？
                    if((crossPoint.x > preCrossPointX) && (crossPoint.x < targetVertex.x)) {
                        leftHorizontallyAdjacentEdges[i] = vertexSet[0];
                        preCrossPointX = crossPoint.x;
                    }
                }
            }
        }
        return leftHorizontallyAdjacentEdges;
    }
    
    // 単純多角形を三角形に分割する
    private void DivideSimpleGeometryToTriangle(){

    }
    // 図形を単純他t角形に分割する
    
    private void DivideComplexToSimpleGeometry(Plane cutter, int length, List<int[]> vertexSetLists, List<Vector3> vertices) {
        Vector2[] vertices2D = ConvertTo2DCoordinates(cutter, vertices)
        VertexGrouping(vertexSetLists, newVertexSetList);
        AssortVertexIndex assortVertexIndex = DecomposeVertexSet(length, newVertexSetList, vertices2D);
        int[] leftHorizontallyAdjacentEdges = SetHorizontallyAdjacentEdges(length, vertexSetLists, vertices2D);

        int[] vertexIndexList = new int[vertices.Length];
        for(int i = 0; i < vertices.Length; i++) {
            vertexIndexList[i] = i;
        }
        // y座標をキーとして持つ配列を作成
        float[] keys = new float[vertices2D.Length];
        for (int i = 0; i < vertices2D.Length; i++) {
            keys[i] = vertices2D[i].y; // 各頂点のy座標をキーにする
        }
        // y座標でソート
        Array.Sort(keys, vertexIndexList);

        // ここからが本番
        foreach(int vertexIndex in vertexIndexList){
        }
    }
    

    // 平面上の頂点を2D座標に変換する関数
    private Vector2[] ConvertTo2DCoordinates(Plane cutter, List<Vector3> vertices)
    {
        Vector2[] result = new Vector2[vertices.Count];
        Vector3 planeNormal = cutter.normal;
        Vector3 planePoint = planeNormal * cutter.distance;

        // 平面の基底ベクトルを計算
        Vector3 u = Vector3.Cross(planeNormal, Vector3.up).normalized;
        if (u.magnitude < 0.001f)
        {
            u = Vector3.Cross(planeNormal, Vector3.right).normalized;
        }
        Vector3 v = Vector3.Cross(planeNormal, u);

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 pointOnPlane = vertices[i] - planePoint;
            float x = Vector3.Dot(pointOnPlane, u);
            float y = Vector3.Dot(pointOnPlane, v);
            result[i] = new Vector2(x, y);
        }

        return result;
    }


    private void CreateObject(Vector3[] vertices, Vector2[] uvs, int[] triangles) {
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

    private void CreateRigidObject(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
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
}



public class AttVertex {
    private Vector2 _vertex;
    public Vector2 Vertex {
        get { return _vertex; }
    }
    private int _horizontalAdjacentEdge;

    public int HorizontalAdjacentEdge {
        get { return _horizontalAdjacentEdge; }
    }
    private List<int> _helperVertexIndex new List<int>();
    public List<int> HelperVertexIndex {
        get { return _helperVertexIndex; }
    }

    public AttVertex(Vector2 vertex, int horizontalAdjacentEdge, int helperVertexIndex) {
        this._vertex = vertex;
        this._horizontalAdjacentEdge = horizontalAdjacentEdge;
        this._helperVertexIndex.Add(helperVertexIndex);
    }
    
    public void AddHelperVertexIndex(int helperVertexIndex) {
        this._helperVertexIndex.Add(helperVertexIndex);
    }
    public int PopHeloperVertexIndex() {
        int helperVertexIndex = this._helperVertexIndex[0];
        this._helperVertexIndex.RemoveAt(0);
        return helperVertexIndex;
    }
}


public class AssortVertexIndex {
    private List<int> _startVertexList = new List<int>();
    public List<int> StartVertexList {
        get { return _startVertexList; }
        set { _startVertexList = value; }
    }
    private List<int> _splitVertexList = new List<int>();
    public List<int> SplitVertexList {
        get { return _splitVertexList; }
        set { _splitVertexList = value; }
    }
    private List<int> _endVertexList = new List<int>();
    public List<int> EndVertexList {
        get { return _endVertexList; }
        set { _endVertexList = value; }
    }
    private List<int> _mergeVertexList = new List<int>();
    public List<int> MergeVertexList {
        get { return _mergeVertexList; }
        set { _mergeVertexList = value; }
    }
}