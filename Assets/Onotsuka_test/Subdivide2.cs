using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static RondomSubdivision;
using TreeEditor;
using Unity.VisualScripting;
using System.Runtime.ConstrainedExecution;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class Subdivide2 : MonoBehaviour {

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
        List<int[]>    vertexSetList       = new List<int[]>();
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
                vertexSetList.Add(vertexSet);

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
        // 座標を切断面で2次元化
        Vector2[] verticesAry2D = ConvertTo2DCoordinates(cutter, newVerticesList);
        DivideComplexToSimpleGeometry(vertexSetList, targetVerticesLength, verticesAry2D);
        // Debug
        // Debug.Log(string.Join(", ", newVertexSetList.Select(obj => obj.ToString())));
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
    private List<List<int>> VertexGrouping(List<int[]> vertexSetList, List<List<int>> vertexGroupsList) {
        // コピーのリストを作成
        List<int[]> remainingVertexSetList = new List<int[]>(vertexSetList);
        // 頂点グループの最初のEdgeの開始点と終点を取得
        int startVertex = remainingVertexSetList[0][0];
        int endVertex = remainingVertexSetList[0][1];
        // 最初のEdgeの頂点を追加し、削除
        List<int> vertexGroup = new List<int>();
        vertexGroup.Add(startVertex);
        vertexGroup.Add(endVertex);
        remainingVertexSetList.RemoveAt(0);
        // 頂点が一周するまでループ
        while (startVertex != endVertex){
            // 残りの頂点リストから、前回の終点から始まるEdgeを探す
            for (int i = 0; i < remainingVertexSetList.Count; i++){
                if (endVertex == remainingVertexSetList[i][0]){
                    // 終点を更新、頂点グループに追加し、削除
                    endVertex = remainingVertexSetList[i][1];
                    vertexGroup.Add(endVertex);
                    remainingVertexSetList.RemoveAt(i);
                    break;
                }
            }
        }
        // まだ処理されていない頂点が残っている場合、再帰的にグループ化を続ける
        if (remainingVertexSetList.Count > 0){
            vertexGroupsList.Add(vertexGroup);
            return VertexGrouping(remainingVertexSetList, vertexGroupsList);
        }
        // 全ての頂点ペアが処理された場合、結果を返す
        return vertexGroupsList;
    }


    // 頂点の情報の詳細分類 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
    private int DecomposeVertex(int preVertexIndex, int nowVertexIndex, int nextVertexIndex, int offsetVertexIndex, Vector2[] verticesAry2D) {
        // 頂点座標の取得
        Vector2 preVertex = verticesAry2D[preVertexIndex - offsetVertexIndex];
        Vector2 nowVertex = verticesAry2D[nowVertexIndex - offsetVertexIndex];
        Vector2 nextVertex = verticesAry2D[nextVertexIndex - offsetVertexIndex];
        // 外積
        DoubleVector2 preToNow = new DoubleVector2(nowVertex - preVertex);
        DoubleVector2 nowToNext = new DoubleVector2(nextVertex - nowVertex);
        double crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
        // 判別
        int assortVertex = 0; // 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
        if ((crossProduct < 0) && (nowVertex.y > preVertex.y) && (nowVertex.y > preVertex.y)) {
            assortVertex = 1; // StartVertex
        } else if ((crossProduct < 0) && (nowVertex.y < preVertex.y) && (nowVertex.y < preVertex.y)) {
            assortVertex = 2; // EndVertexList
        } else if ((crossProduct < 0) && (nowVertex.y > preVertex.y) && (nowVertex.y > preVertex.y)) {
            assortVertex = 3; // SplitVertex 
        } else if ((crossProduct > 0) && (nowVertex.y < preVertex.y) && (nowVertex.y < preVertex.y)) {
            assortVertex = 4; // MergeVertexList
        } else {
            assortVertex = 0; // 一般のVertex
        }
        return assortVertex;
    }

    // ある頂点に対して水平方向右に隣接する辺を求める．
    private int SearchHorizontallyAdjacentEdgeStartVertexIndex(int vertexIndex, int offsetVertexIndex, List<int[]> vertexSetList, Vector2[] verticesAry2D) {
        int horizontallyAdjacentEdgeStartVertexIndex = 0;
        // 現在の見ている頂点
        Vector2 targetVertex = verticesAry2D[vertexIndex - offsetVertexIndex];
        // 各辺について水平線に交差しているかを判定する
        foreach (int[] vertexSet in vertexSetList) {
            // 一番近い辺のx座標を初期化
            float nearestCrossPointX = -Mathf.Infinity;
            // 現在見ている辺の頂点
            Vector2 startVertex = verticesAry2D[vertexSet[0] - offsetVertexIndex];
            Vector2 endVertex = verticesAry2D[vertexSet[1] - offsetVertexIndex];
            // startVertex と endVertex の間に targetVertex があるかどうか？ つまり交差してるかってこと
            if ((startVertex.y <= targetVertex.y && endVertex.y > targetVertex.y) || (startVertex.y > targetVertex.y && endVertex.y <= targetVertex.y)) {
                // 水平線と辺の交点を求める
                float y = targetVertex.y;
                //  x = (y - y1)(x2 - x1) / (y2 - y1) + x1;
                float x = (y - startVertex.y) * (endVertex.x - startVertex.x) / (endVertex.y - startVertex.y) + startVertex.x;
                Vector2 crossPoint = new Vector2(x, y);
                // それは右ですか？しかも一番近いですか？
                if ((crossPoint.x < nearestCrossPointX) && (crossPoint.x > targetVertex.x)) {
                    horizontallyAdjacentEdgeStartVertexIndex = vertexSet[0];
                    nearestCrossPointX = crossPoint.x;
                }
            }
        }
        return horizontallyAdjacentEdgeStartVertexIndex;
    }

    // ある頂点を始点とする辺に対して、始点と終点のうちY座標の大きい方を補助頂点として設定する。
    private int InitializeHelperVertexIndex(int startVertexIndex, int endVertexIndex, int offsetVertexIndex, Vector2[] verticesAry2D) {
        int helperVertexIndex = 0;
        // 頂点座標の取得
        Vector2 startVertex = verticesAry2D[startVertexIndex - offsetVertexIndex];
        Vector2 endVertex = verticesAry2D[endVertexIndex - offsetVertexIndex];
        if (startVertex.y > endVertex.y) {
            helperVertexIndex = startVertexIndex;
        } else {
            helperVertexIndex = endVertexIndex;
        }
        return helperVertexIndex;
    }


    // 単純多角形を三角形に分割する
    private void DivideSimpleGeometryToTriangle() {

}

    // 図形を単純多角形に分割する

    private void DivideComplexToSimpleGeometry(List<int[]> vertexSetList, int offsetVertexIndex, Vector2[] verticesAry2D) {
        // 頂点グループを生成
        List<List<int>> vertexGroupsList = new List<List<int>>();
        VertexGrouping(vertexSetList, vertexGroupsList);
        // 各頂点に必要な情報の作成　隣接辺　ヘルパー頂点　一つ後ろの頂点 分類
        int[] horizontallyAdjacentEdgesAry = new int[verticesAry2D.Length];  // 水平右側隣接辺配列
        List<int>[] helperVertexIndexListAry = new List<int>[verticesAry2D.Length];  // ヘルパー頂点リストの配列
        int[] VertexAssortAry = new int[verticesAry2D.Length];  // 頂点

        foreach (List<int> vertexGroup in vertexGroupsList) {
            for (int i = 0; i < vertexGroup.Count-1; i++) {
                int vertexIndex = vertexGroup[i];
                // 水平右側隣接辺を取得
                horizontallyAdjacentEdgesAry[vertexIndex] = SearchHorizontallyAdjacentEdgeStartVertexIndex(vertexIndex, offsetVertexIndex, vertexSetList, verticesAry2D);
                // ヘルパー頂点を初期化
                int startVertexIndex = vertexGroup[i];
                int endVertexIndex = vertexGroup[i+1];
                int helperVertexIndex = InitializeHelperVertexIndex(startVertexIndex, endVertexIndex, offsetVertexIndex, verticesAry2D);
                helperVertexIndexListAry[startVertexIndex] = new List<int>() { helperVertexIndex };
                // 頂点を分類
                int preVertexIndex = vertexGroup[i];
                int nowVertexIndex = vertexGroup[i+1];
                int nextVertexIndex;
                if (i + 2 < vertexGroup.Count) {
                    nextVertexIndex = vertexGroup[i + 2];
                } else {
                    nextVertexIndex = vertexGroup[0];
                }
                VertexAssortAry[nowVertexIndex] = DecomposeVertex(offsetVertexIndex, preVertexIndex, nowVertexIndex, nextVertexIndex, verticesAry2D);
            }
        }

        // 単純多角形に分割するための辺を追加した辺リスト
        List<int[]> newVertexSetList = new List<int[]>(vertexSetList);

        // endVertexのY座標でソートした辺リストを作成する
        List<int[]> sortedVertexSetList = new List<int[]>(vertexSetList);
        sortedVertexSetList.Sort((a, b) => Math.Sign(verticesAry2D[a[1]].y - verticesAry2D[b[1]].y));

        // y座標が一番大きい頂点から順に処理
        foreach (int[] vertexSet in sortedVertexSetList) {
            int preVertexIndex = vertexSet[0];
            int vertexIndex = vertexSet[1];
            // 頂点の分類  0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
            if (VertexAssortAry[vertexIndex] == 0) {
                // 頂点が一般点の場合
                // その点のヘルパー頂点を見る。
                List<int> helperVertexIndexList = helperVertexIndexListAry[vertexIndex];
                int helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                // Y座標がヘルパー頂点より小さい場合は上向き向き
                if (verticesAry2D[vertexIndex].y > verticesAry2D[helperVertexIndex].y) {
                    // 上向きの場合は左側（X軸方向正）が内部となる。
                    // 水平隣接辺のヘルパー頂点を調べる。
                    int hEdgeIndex = horizontallyAdjacentEdgesAry[vertexIndex];
                    helperVertexIndexList = helperVertexIndexListAry[hEdgeIndex];
                    helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                    // 統合点なら対角線を引き、ヘルパー頂点を消す。
                    if (VertexAssortAry[helperVertexIndex] == 4) {
                        // 対角線を結ぶ
                        int[] newEdge = new int[] { vertexIndex, helperVertexIndex };
                        int[] newEdgeR = new int[] { helperVertexIndex, vertexIndex };
                        newVertexSetList.Add(newEdge);
                        newVertexSetList.Add(newEdgeR);
                        helperVertexIndexList.RemoveAt(helperVertexIndexList.Count - 1);
                    }
                } else {
                    // 下向きの場合は左側（X軸方向負）が内部となる。
                    // ひとつ前の辺のヘルパー頂点を調べる。
                    helperVertexIndexList = helperVertexIndexListAry[preVertexIndex];
                    helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                    // 統合点なら対角線を引き、ヘルパー頂点を消す。
                    if (VertexAssortAry[helperVertexIndex] == 4) {
                        // 対角線を結ぶ
                        int[] newEdge = new int[] { vertexIndex, helperVertexIndex };
                        int[] newEdgeR = new int[] { helperVertexIndex, vertexIndex };
                        newVertexSetList.Add(newEdge);
                        newVertexSetList.Add(newEdgeR);
                        helperVertexIndexList.RemoveAt(helperVertexIndexList.Count - 1);
                    }
                }

            } else if (VertexAssortAry[vertexIndex] == 1) {
                // 頂点が出発点の場合 必ず下向き
                // ひとつ前の辺のヘルパー頂点を調べる。
                List<int> helperVertexIndexList = helperVertexIndexListAry[preVertexIndex];
                int helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                // 統合点なら対角線を引き、ヘルパー頂点を消す。
                if (VertexAssortAry[helperVertexIndex] == 4) {
                    // 対角線を結ぶ
                    int[] newEdge = new int[] { vertexIndex, helperVertexIndex };
                    int[] newEdgeR = new int[] { helperVertexIndex, vertexIndex };
                    newVertexSetList.Add(newEdge);
                    newVertexSetList.Add(newEdgeR);
                    helperVertexIndexList.RemoveAt(helperVertexIndexList.Count - 1);
                }
            } else if (VertexAssortAry[vertexIndex] == 2) {
                // 頂点が最終点の場合　必ず上向き
                // 水平隣接辺のヘルパー頂点を調べる。
                int hEdgeIndex = horizontallyAdjacentEdgesAry[vertexIndex];
                List<int> helperVertexIndexList = helperVertexIndexListAry[hEdgeIndex];
                int helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                // 統合点なら対角線を引き、ヘルパー頂点を消す。
                if (VertexAssortAry[helperVertexIndex] == 4) {
                    // 対角線を結ぶ
                    int[] newEdge = new int[] { vertexIndex, helperVertexIndex };
                    int[] newEdgeR = new int[] { helperVertexIndex, vertexIndex };
                    newVertexSetList.Add(newEdge);
                    newVertexSetList.Add(newEdgeR);
                    helperVertexIndexList.RemoveAt(helperVertexIndexList.Count - 1);
                }
            } else if (VertexAssortAry[vertexIndex] == 3) {
                // 頂点が分割点の場合
                // 水平方向の隣接辺を見る。。
                int hEdgeIndex = horizontallyAdjacentEdgesAry[vertexIndex];
                // その辺のヘルパー頂点を見る。
                List<int> helperVertexIndexList = helperVertexIndexListAry[hEdgeIndex];
                int helperVertexIndex = helperVertexIndexList[helperVertexIndexList.Count - 1];
                // ヘルパー頂点と対角線を結ぶ
                int[] newEdge = new int[] { vertexIndex, helperVertexIndex };
                int[] newEdgeR = new int[] { helperVertexIndex, vertexIndex };
                newVertexSetList.Add(newEdge);
                newVertexSetList.Add(newEdgeR);
                // ヘルパー頂点が統合点なら消す。
                if (VertexAssortAry[helperVertexIndex] == 4) {
                    helperVertexIndexList.RemoveAt(helperVertexIndexList.Count - 1);
                }

            } else if (VertexAssortAry[vertexIndex] == 4) {
                // 頂点が統合点の場合
                // 水平方向の隣接辺を見る。その辺のヘルパー頂点リストに、自分を追加
                int hEdgeIndex = horizontallyAdjacentEdgesAry[vertexIndex];
                 helperVertexIndexListAry[hEdgeIndex].Add(vertexIndex);
            }
            

        }
        // 頂点グループを生成
        List<List<int>> newVertexGroupsList = new List<List<int>>();
        VertexGrouping(newVertexSetList, newVertexGroupsList);
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

    private int _preVertexIndex;
    public int PreVertexIndex {
        get { return _preVertexIndex; }
    }

    private List<int> _nextVertexIndex = new List<int>();
    public List<int> NextVertexIndex {
        get { return _nextVertexIndex; }
    }

    private int _horizontalAdjacentEdge;
    public int HorizontalAdjacentEdge {
        get { return _horizontalAdjacentEdge; }
    }

    private List<int> _helperVertexIndex = new List<int>();
    public List<int> HelperVertexIndex {
        get { return _helperVertexIndex; }
    }

    public AttVertex(Vector2 vertex, int preVertexIndex, int nextVertexIndex, int horizontalAdjacentEdge, int helperVertexIndex) {
        this._vertex = vertex;
        this._preVertexIndex = preVertexIndex;
        this._nextVertexIndex.Add(nextVertexIndex);
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

    public void AddNextVertexIndex(int nextVertexIndex) {
        this._nextVertexIndex.Add(nextVertexIndex);
    }
}








/*
main(){
    for v in List{
        二次元化(v);
    }
    for v in List{
        隣接辺探索(x);
    }
    for v in List{
        ヘルパー頂点設定(v);
    }
}

#######################################

main(){
    for v in List{
        二次元化(v);
        隣接辺探索(x);
        ヘルパー頂点設定(v);
    }
}
二次元化(v){}
隣接辺探索(v){}
ヘルパー頂点設定(v){}
*/