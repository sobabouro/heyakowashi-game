using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static RondomSubdivision;
using TreeEditor;
using Unity.VisualScripting;
using System.Runtime.ConstrainedExecution;
using System.Drawing;
using System.Windows.Forms;

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
        targetVertices = targetMesh.vertices;
        targetNormals = targetMesh.normals;
        targetUVs = targetMesh.uv;

        // 切断対象のオブジェクトの情報操作用
        int targetVerticesLength = targetVertices.Length;
        List<int> irrelevantTriangles = new List<int>();
        List<Vector3> newVerticesList = new List<Vector3>();
        List<int[]> vertexSetList = new List<int[]>();
        // 切断面左側のオブジェクトのメッシュ情報
        List<int> leftTriangles = new List<int>();
        List<Vector3> leftVertices = new List<Vector3>();
        List<Vector3> leftNormals = new List<Vector3>();
        List<Vector2> leftUVs = new List<Vector2>();
        // 切断面右側のオブジェクトのメッシュ情報
        List<int> rightTriangles = new List<int>();
        List<Vector3> rightVertices = new List<Vector3>();
        List<Vector3> rightNormals = new List<Vector3>();
        List<Vector2> rightUVs = new List<Vector2>();

        // 切断対象のオブジェクトの各ポリゴンの左右判定用
        List<int> numLeftVertices = new List<int>();
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
            } else {
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
                int[] vertexSet = new int[] { newVertexIndexSV, newVertexIndexLV };
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
        // Debug.Log(string.Join(", ", newVerticesList.Select(obj => obj.ToString())));

        //　五角系In五角系のテストケース
        //newVerticesList = new List<Vector3>{
        //new Vector3(0, 2, 0),
        //new Vector3(0, 0.618f, 1.902f),
        //new Vector3(0, -1.618f, 1.176f),
        //new Vector3(0, -1.618f, -1.176f),
        //new Vector3(0, 0.618f, -1.902f),
        //new Vector3(0, 1, 0),
        //new Vector3(0, 0.309f, 0.951f),
        //new Vector3(0, -0.809f, 0.588f),
        //new Vector3(0, -0.809f, -0.588f),
        //new Vector3(0, 0.309f, -0.951f)
        //};
        //vertexSetList = new List<int[]>{
        //    new int[]{ targetVerticesLength+0, targetVerticesLength + 1 },
        //    new int[]{ targetVerticesLength + 1,targetVerticesLength + 2 },
        //    new int[]{ targetVerticesLength + 2,targetVerticesLength + 3 },
        //    new int[]{ targetVerticesLength + 3,targetVerticesLength + 4 },
        //    new int[]{ targetVerticesLength + 4,targetVerticesLength + 0 },
        //    new int[]{ targetVerticesLength + 9,targetVerticesLength + 8 },
        //    new int[]{ targetVerticesLength + 8,targetVerticesLength + 7 },
        //    new int[]{ targetVerticesLength + 7,targetVerticesLength + 6 },
        //    new int[]{ targetVerticesLength + 6,targetVerticesLength + 5 },
        //    new int[]{ targetVerticesLength + 5,targetVerticesLength + 9 }
        //};

        Vector2[] verticesAry2D = ConvertTo2DCoordinates(cutter, newVerticesList);
        DivideComplexGeometryToTriangle(vertexSetList, targetVerticesLength, verticesAry2D);
    }

    // ポリゴンの頂点番号を，孤独な頂点を先頭に，表裏情報をもつ順番に並び替える
    (bool rtlf, int newIndex1, Vector3 lonelyVertex, int newIndex2, Vector3 startPairVertex, int newIndex3, Vector3 lastPairVertex) SortIndex(int index1, bool vertexTruthValue1, Vector3 vertex1, int index2, bool vertexTruthValue2, Vector3 vertex2, int index3, bool vertexTruthValue3, Vector3 vertex3) {
        // 孤独な頂点が無限平面の右側にある場合
        if (vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index1, vertex1, index2, vertex2, index3, vertex3);
        } else if (!vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
        } else if (!vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
            bool rtlf = true;
            return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
        }
          // 孤独な頂点が無限平面の左側にある頂点
          else if (vertexTruthValue1 && vertexTruthValue2 && !vertexTruthValue3) {
            bool rtlf = false;
            return (rtlf, index3, vertex3, index1, vertex1, index2, vertex2);
        } else if (vertexTruthValue1 && !vertexTruthValue2 && vertexTruthValue3) {
            bool rtlf = false;
            return (rtlf, index2, vertex2, index3, vertex3, index1, vertex1);
        } else { // (!vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3)
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
        } else {
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
        while (startVertex != endVertex) {
            // 残りの頂点リストから、前回の終点から始まるEdgeを探す
            for (int i = 0; i < remainingVertexSetList.Count; i++) {
                if (endVertex == remainingVertexSetList[i][0]) {
                    // 終点を更新、頂点グループに追加し、削除
                    endVertex = remainingVertexSetList[i][1];
                    vertexGroup.Add(endVertex);
                    remainingVertexSetList.RemoveAt(i);
                    break;
                }
            }
        }
        vertexGroupsList.Add(vertexGroup);
        // まだ処理されていない頂点が残っている場合、再帰的にグループ化を続ける
        if (remainingVertexSetList.Count > 0) {
            return VertexGrouping(remainingVertexSetList, vertexGroupsList);
        }
        // 全ての頂点ペアが処理された場合、結果を返す
        return vertexGroupsList;
    }

    // 対角線を追加した頂点セットリストから，ペア同士の探索を行い，頂点グループを生成する
    private List<List<int>> ReVertexGrouping(AttVertex[] attVertexAry, List<List<int>> vertexGroupsList) {
        // コピーのリストを作成
        List<AttVertex> remainingAttVertexList = new List<AttVertex>();
        foreach (AttVertex attVertex in attVertexAry) {
            remainingAttVertexList.Add(new AttVertex(attVertex));
        }
        // 頂点グループの最初のEdgeの開始点と終点を取得
        Vector2 preAttVertexPos = remainingAttVertexList[0].PreAttVertex.Position;
        AttVertex nowAttVertex = remainingAttVertexList[0];
        AttVertex nextAttVertex = remainingAttVertexList[0].NextAttVertex[0];
        // 最初のEdgeの頂点を追加し、削除
        List<int> vertexGroup = new List<int>();
        vertexGroup.Add(nowAttVertex.Index);
        vertexGroup.Add(nextAttVertex.Index);
        remainingAttVertexList.RemoveAt(0);
        int startVertexIndex = nowAttVertex.Index;
        int endVertexIndex = nextAttVertex.Index;
        // 頂点が一周するまでループ
        while (startVertexIndex != endVertexIndex) {
            // 残りの頂点リストから、前回の終点から始まる、最も右折する辺を求める。
            for (int i = 0; i < remainingAttVertexList.Count; i++) {
                AttVertex attVertex = remainingAttVertexList[i];
                if (endVertexIndex == attVertex.Index) {
                    nextAttVertex = attVertex.MinAngleNextVertex(preAttVertexPos);
                    // 終点を更新、頂点グループに追加し、削除
                    endVertexIndex = nextAttVertex.Index;
                    preAttVertexPos = attVertex.Position;
                    vertexGroup.Add(endVertexIndex);
                    int nextAttVertexNum = attVertex.NextAttVertex.Count;
                    if (nextAttVertexNum == 1) {
                        remainingAttVertexList.RemoveAt(i);
                    } else {
                        attVertex.NextAttVertex.Remove(nextAttVertex);
                    }
                }
            }
            // Debug.Log("remainingVertexSetList: " + string.Join(", ", remainingVertexSetList.Select(a => "(" + string.Join(", ", a.Select(b => b.ToString())) + ")")));
        }
        vertexGroupsList.Add(vertexGroup);
        // まだ処理されていない頂点が残っている場合、再帰的にグループ化を続ける
        // if (remainingVertexSetList.Count > 0) {
        //     return VertexGrouping(remainingVertexSetList, vertexGroupsList);
        // }
        // 全ての頂点ペアが処理された場合、結果を返す
        return vertexGroupsList;
    }



    // ある頂点に対して水平方向右に隣接する辺を求める．
    private AttVertex[] CreateAttVertexAry(List<List<int>> vertexGroupsList, int offsetVertexIndex, Vector2[] verticesAry2D) { 

        AttVertex[] attVertexAry = new AttVertex[verticesAry2D.Length];
        foreach (List<int> vertexGroup in vertexGroupsList) {
            //  最初の要素の例外
            int preVertexIndex = vertexGroup[vertexGroup.Count - 2];
            int nowVertexIndex = vertexGroup[0];
            int nextVertexIndex = vertexGroup[1];
            // 頂点の座標
            Vector2 VertexPosition = verticesAry2D[nowVertexIndex - offsetVertexIndex];
            AttVertex StartAttVertex = new AttVertex(
                VertexPosition,
                nowVertexIndex
                );
            attVertexAry[nowVertexIndex - offsetVertexIndex] = StartAttVertex;
            AttVertex preAttVertex = StartAttVertex;
            // ２番目以降の要素の処理
            for (int i = 0; i < vertexGroup.Count - 1; i++) {
                preVertexIndex = vertexGroup[i - 1];
                nowVertexIndex = vertexGroup[i];
                nextVertexIndex = vertexGroup[i + 1];
                // 頂点の座標
                VertexPosition = verticesAry2D[nowVertexIndex - offsetVertexIndex];
                AttVertex attVertex = new AttVertex(
                    VertexPosition,
                    nowVertexIndex
                    );
                attVertex.PreAttVertex = preAttVertex;
                attVertexAry[nowVertexIndex - offsetVertexIndex] = attVertex;
                preAttVertex.AddNextAttVertex(attVertex);
                preAttVertex = attVertex;
            }
            StartAttVertex.PreAttVertex = preAttVertex;
            preAttVertex.AddNextAttVertex(StartAttVertex);
        }
        return attVertexAry;
    }


    // ある頂点に対して水平方向右に隣接する辺を求める．
    private int SearchHorizontallyAdjacentEdgeStartVertexIndex(AttVertex targetAttVertex, int offsetVertexIndex, AttVertex[] attVertexAry) {
        int horizontallyAdjacentEdgeStartVertexIndex = 0;
        // 現在の見ている頂点の座標
        Vector2 targetVertex = targetAttVertex.Position;
        // 各辺について水平線に交差しているかを判定する
        foreach (AttVertex attVertex in attVertexAry) {
            // 一番近い辺のx座標を初期化
            float nearestCrossPointX = Mathf.Infinity;
            // 現在見ている辺の頂点
            Vector2 startVertex = attVertex.Position;
            Vector2 endVertex = attVertex.NextAttVertex[0].Position;
            // startVertex と endVertex の間に targetVertex があるかどうか？ つまり交差してるかってこと
            if ((startVertex.y <= targetVertex.y && endVertex.y > targetVertex.y) || (startVertex.y > targetVertex.y && endVertex.y <= targetVertex.y)) {
                // 水平線と辺の交点を求める
                float y = targetVertex.y;
                //  x = (y - y1)(x2 - x1) / (y2 - y1) + x1;
                float x = (y - startVertex.y) * (endVertex.x - startVertex.x) / (endVertex.y - startVertex.y) + startVertex.x;
                Vector2 crossPoint = new Vector2(x, y);
                // Debug.Log(vertexIndex.ToString() + targetVertex.ToString() +" -> "+ vertexSet[0].ToString() + startVertex.ToString() + vertexSet[1].ToString() + endVertex.ToString() + " 交点="+ crossPoint);
                // それは右ですか？しかも一番近いですか？
                if ((crossPoint.x < nearestCrossPointX) && (crossPoint.x > targetVertex.x)) {
                    horizontallyAdjacentEdgeStartVertexIndex = targetAttVertex.Index;
                    nearestCrossPointX = crossPoint.x;
                }
            }
        }
        return horizontallyAdjacentEdgeStartVertexIndex;
    }



    // 単純多角形を三角形に分割する
    private List<int[]> DivideSimpleGeometryToTriangle(List<int> simpleGeometryVertexGroup, int offsetVertexIndex, Vector2[] verticesAry2D) {
        int[][] vertexSetAry = new int[verticesAry2D.Length][];

        // Y座標でソートした頂点番号リストを作成する
        int[] sortedVertxIndexAry = new int[simpleGeometryVertexGroup.Count - 1];
        float[] keys = new float[simpleGeometryVertexGroup.Count - 1];

        for (int i = 0; i < simpleGeometryVertexGroup.Count - 1; i++) {
            int preVertexIndex = (i - 1 >= 0 ? simpleGeometryVertexGroup[i - 1] : simpleGeometryVertexGroup[simpleGeometryVertexGroup.Count - 2]);
            int nowVertexIndex = simpleGeometryVertexGroup[i];
            int nextVertexIndex = simpleGeometryVertexGroup[i + 1];
            vertexSetAry[nowVertexIndex - offsetVertexIndex] = new int[] { preVertexIndex, nowVertexIndex, nextVertexIndex };

            sortedVertxIndexAry[i] = nowVertexIndex;
            // 注意点: 1_000_000 は適当なスケーリングファクターで、Y軸の値がX軸の値よりも影響力を持つようにしています。Y軸とX軸の値の範囲に応じて調整が必要です。
            keys[i] = verticesAry2D[nowVertexIndex - offsetVertexIndex].y * 1_000_000 + verticesAry2D[i].x; ; // nowVertexIndexのY座標
        }

        Array.Sort(keys, sortedVertxIndexAry);

        // CompareToでソート
        // Array.Sort(sortedVertxIndexAry, (a, b) => {
        //     if (verticesAry2D[a].y != verticesAry2D[b].y)
        //         return verticesAry2D[b].y.CompareTo(verticesAry2D[a].y); // Yの降順
        //     return verticesAry2D[b].x.CompareTo(verticesAry2D[a].x);     // Xの降順
        // });

        List<int[]> triangleList = new List<int[]>();
        // y座標が一番大きい頂点から順に処理
        List<int[]> vertexSetStack = new List<int[]>();
        for (int i = 0; i < sortedVertxIndexAry.Length - 2; i++) {
            // 一番上の頂点、次に高い頂点、その次に高い頂点
            int[] vertexSet1 = vertexSetAry[sortedVertxIndexAry[i] - offsetVertexIndex];
            int[] vertexSet2 = vertexSetAry[sortedVertxIndexAry[i + 1] - offsetVertexIndex];
            int[] vertexSet3 = vertexSetAry[sortedVertxIndexAry[i + 2] - offsetVertexIndex];
            // Debug.Log("vertexSet: " + string.Join(", ", targetVertexSet.Select(a => a.ToString())));
            // それぞれのY座標が等しかったら一直線なので三角形を作成しない。そうでないなら三角形を作成する。
            float y1 = verticesAry2D[vertexSet1[1] - offsetVertexIndex].y;
            float y2 = verticesAry2D[vertexSet2[1] - offsetVertexIndex].y;
            float y3 = verticesAry2D[vertexSet3[1] - offsetVertexIndex].y;
            // Debug.Log(y1.ToString() + y2.ToString() + y3.ToString() + (y1 != y2 || y2 != y3 || y3 != y1).ToString());
            if (y1 != y2 || y2 != y3 || y3 != y1) {
                triangleList.Add(new int[] { vertexSet1[1], vertexSet2[1], vertexSet3[1] });
            }

            // 頂点をカットして、前後の頂点を辺でつなぎなおす。
            int[] cutVertexSet = null;
            // カットする頂点の場合分け （２番目に高い点と３番目に高い点がつながっているかどうか）
            int vertexSet2PreIndex = vertexSet2[0];
            int vertexSet2NextIndex = vertexSet2[2];
            int vertexSet3Index = vertexSet3[1];
            if (vertexSet2PreIndex == vertexSet3Index || vertexSet2NextIndex == vertexSet3Index) {
                //　２番目に高い点と３番目に高い点がつながっている場合、２番目に高い点をカット
                cutVertexSet = vertexSet2;
                // 次ループでみる一番高い頂点は変わらない。
                sortedVertxIndexAry[i + 1] = sortedVertxIndexAry[i];
            } else {
                //　２番目に高い点と３番目に高い点がつながっていない場合、１番目に高い点をカット
                cutVertexSet = vertexSet1;
            }
            int[] preVertexSet = vertexSetAry[cutVertexSet[0] - offsetVertexIndex];
            preVertexSet[2] = cutVertexSet[2];
            // Debug.Log("preVertexSet: " + string.Join(", ", preVertexSet.Select(a => a.ToString())));
            int[] nextVertexSet = vertexSetAry[cutVertexSet[2] - offsetVertexIndex];
            nextVertexSet[0] = cutVertexSet[0];
            // Debug.Log("nextVertexSet: " + string.Join(", ", nextVertexSet.Select(a => a.ToString())));
        }
        Debug.Log("triangleList: " + string.Join(", ", triangleList.Select(a => "(" + string.Join(", ", a.Select(b => b.ToString())) + ")")));

        return triangleList;
    }

    // 図形を単純多角形に分割する

    private List<List<int>> DivideComplexToSimpleGeometry(List<int[]> vertexSetList, int offsetVertexIndex, Vector2[] verticesAry2D) {

        // Debug.Log(string.Join(", ", verticesAry2D.Select(obj => obj.ToString())));
        // (0.00, 0.50, 0.50), (0.00, 0.00, 0.50), (0.00, -0.50, 0.50), (0.00, 0.50, -0.50), (0.00, 0.50, 0.00), (0.00, -0.50, -0.50), (0.00, 0.00, -0.50), (0.00, -0.50, 0.00)
        // (0.50, -0.50),      (0.50, 0.00),       (0.50, 0.50),         (-0.50, -0.50),     (0.00, -0.50),      (-0.50, 0.50),        (-0.50, 0.00),       (0.00, 0.50)

        // 頂点グループを生成
        List<List<int>> vertexGroupsList = new List<List<int>>();
        VertexGrouping(vertexSetList, vertexGroupsList);

        foreach (List<int> vertexGroup in vertexGroupsList) {
            Debug.Log("In vertexGroup " + string.Join(", ", vertexGroup.Select(obj => obj.ToString())));
        }

        AttVertex[] attVertexAry = CreateAttVertexAry(vertexGroupsList, offsetVertexIndex, verticesAry2D);

        foreach (AttVertex attVertex in attVertexAry) {
            // 水平隣接辺を取得
            attVertex.HorizontalAdjacentEdge = SearchHorizontallyAdjacentEdgeStartVertexIndex(attVertex, offsetVertexIndex, attVertexAry);
            // ヘルパー頂点を初期化
            attVertex.InitializeHelperVertexIndex();
            // 頂点を分類
            attVertex.DecomposeVertex();
        }

        Debug.Log("helperVertexIndex: " + string.Join(", ", attVertexAry.Select(a => "(" + string.Join(", ", a.HelperVertexIndexList.Select(b => b.ToString())) + ")")));
        Debug.Log("vertexAssort: " + string.Join(", ", attVertexAry.Select(obj => obj.Assort.ToString())));


        // Y座標でソートした頂点番号を作成する
        int[] sortedVertxIndexAry = new int[attVertexAry.Length];
        float[] keys = new float[attVertexAry.Length];
        for (int i = 0; i < attVertexAry.Length; i++) {
            sortedVertxIndexAry[i] = i;
            keys[i] = attVertexAry[i].Position.y;
        }
        Array.Sort(keys, sortedVertxIndexAry);

        // ここからが本番
        // y座標が一番大きい頂点から順に処理
        foreach (int vertexIndex in sortedVertxIndexAry) {
            // 今見る頂点
            AttVertex targetAttVertex = attVertexAry[vertexIndex];
            // 頂点の分類  0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
            // Debug.Log("VertexAssort: " + targetAttVertex.Assort.ToString());
            if ((targetAttVertex.Assort == 0) && !(targetAttVertex.EdgeDirectionUPorDown())) {
                // Debug.Log("下向き");
                // 分割点でも統合点でもない下向きの辺を持つ頂点の場合 // 頂点から伸びる辺が下向きの場合は右側（X軸方向負）が内部となる。
                // ひとつ前の辺の最新のヘルパー頂点を調べる。
                AttVertex preAttVertex = targetAttVertex.PreAttVertex;
                // Debug.Log("preAttVertex.Index: " + preAttVertex.Index.ToString());
                AttVertex preAttVertexHelper = attVertexAry[preAttVertex.LatestHeloperVertexIndex()];
                // Debug.Log("preAttVertexHelper.Index: " + preAttVertexHelper.Index.ToString());
                // ヘルパー頂点が統合点なら対角線を双方向に引き、ヘルパー頂点を消す。
                if (preAttVertexHelper.Assort == 4) {
                    // Debug.Log("Add Edge");
                    targetAttVertex.AddNextAttVertex(preAttVertexHelper);
                    preAttVertexHelper.AddNextAttVertex(targetAttVertex);
                    preAttVertex.RemoveLatestHeloperVertexIndex();
                }
            } else if ((targetAttVertex.Assort == 0) && (targetAttVertex.EdgeDirectionUPorDown())) {
                /// Debug.Log("上向き");
                // 分割点でも統合点でもない上向きの辺を持つ頂点の場合 // 頂点から伸びる辺が上向きの場合は左側（X軸方向正）が内部となる。
                // 水平隣接辺の最新のヘルパー頂点を調べる。

                // 水平右側隣接辺を取得
                AttVertex haeAttVertex = attVertexAry[targetAttVertex.HorizontalAdjacentEdge];
                // Debug.Log("haeAttVertex.Index: " + haeAttVertex.Index.ToString());
                AttVertex haeAttVertexHelper = attVertexAry[haeAttVertex.LatestHeloperVertexIndex()];
                // Debug.Log("haeAttVertexHelper.Index: " + haeAttVertexHelper.Index.ToString());
                // ヘルパー頂点が統合点なら対角線を引き、ヘルパー頂点を消す。
                if (haeAttVertexHelper.Assort == 4) {
                    // Debug.Log("Add Edge");
                    targetAttVertex.AddNextAttVertex(haeAttVertexHelper);
                    haeAttVertexHelper.AddNextAttVertex(targetAttVertex);
                    haeAttVertex.RemoveLatestHeloperVertexIndex();
                }
            } else if (targetAttVertex.Assort == 3) {
                // 頂点が分割点の場合
                // 水平隣接辺の最新のヘルパー頂点を調べる。
                AttVertex haeAttVertex = attVertexAry[targetAttVertex.HorizontalAdjacentEdge];
                // Debug.Log("haeAttVertex.Index: " + haeAttVertex.Index.ToString());
                AttVertex haeAttVertexHelper = attVertexAry[haeAttVertex.LatestHeloperVertexIndex()];
                // Debug.Log("haeAttVertexHelper.Index: " + haeAttVertexHelper.Index.ToString());
                // ヘルパー頂点と対角線を結ぶ
                // Debug.Log("Add Edge");
                targetAttVertex.AddNextAttVertex(haeAttVertexHelper);
                haeAttVertexHelper.AddNextAttVertex(targetAttVertex);
                // ヘルパー頂点が統合点なら消すAddNextAttVertex
                if (haeAttVertexHelper.Assort == 4) {
                    haeAttVertex.RemoveLatestHeloperVertexIndex();
                }
            } else if (targetAttVertex.Assort == 4) {
                // 頂点が統合点の場合
                // 水平隣接辺のヘルパー頂点リストに、自分を追加
                AttVertex haeAttVertex = attVertexAry[targetAttVertex.HorizontalAdjacentEdge];
                // Debug.Log("haeAttVertex.Index: " + haeAttVertex.Index.ToString());
                haeAttVertex.AddHelperVertexIndex(targetAttVertex.Index);
                // Debug.Log("haeAttVertex.HelperVertexIndexList: " + string.Join(", ", haeAttVertex.HelperVertexIndexList.Select(a => a.ToString())));
            }
        }
        // 頂点グループを生成
        List<List<int>> newVertexGroupsList = new List<List<int>>();
        ReVertexGrouping(attVertexAry, newVertexGroupsList);

        // Debug.Log("newVertexSetList " + string.Join(", ", newVertexSetList.Select(a => "(" + string.Join(", ", a.Select(b => b.ToString())) + ")")));

        foreach (List<int> vertexGroup in newVertexGroupsList) {
            Debug.Log("Out vertexGroup " + string.Join(", ", vertexGroup.Select(obj => obj.ToString())));
        }
        return newVertexGroupsList;
    }


    private List<int[]> DivideComplexGeometryToTriangle(List<int[]> vertexSetList, int offsetVertexIndex, Vector2[] verticesAry2D) {
        List<int[]> allTriangleList = new List<int[]>();
        List<List<int>> newVertexGroupsList = DivideComplexToSimpleGeometry(vertexSetList, offsetVertexIndex, verticesAry2D);
        foreach (List<int> vertexGroup in newVertexGroupsList) {
            List<int[]> triangleList = DivideSimpleGeometryToTriangle(vertexGroup, offsetVertexIndex, verticesAry2D);
            allTriangleList.AddRange(triangleList);
        }
        Debug.Log("allTriangleList: " + string.Join(", ", allTriangleList.Select(a => "(" + string.Join(", ", a.Select(b => b.ToString())) + ")")));

        return allTriangleList;
    }


    // 平面上の頂点を2D座標に変換する関数
    private Vector2[] ConvertTo2DCoordinates(Plane cutter, List<Vector3> vertices) {
        Vector2[] result = new Vector2[vertices.Count];
        Vector3 planeNormal = cutter.normal;
        Vector3 planePoint = planeNormal * cutter.distance;

        // 平面の基底ベクトルを計算
        Vector3 u = Vector3.Cross(planeNormal, Vector3.up).normalized;
        if (u.magnitude < 0.001f) {
            u = Vector3.Cross(planeNormal, Vector3.right).normalized;
        }
        Vector3 v = Vector3.Cross(planeNormal, u);

        for (int i = 0; i < vertices.Count; i++) {
            Vector3 pointOnPlane = vertices[i] - planePoint;
            float x = Vector3.Dot(pointOnPlane, u);
            float y = Vector3.Dot(pointOnPlane, v);
            result[i] = new Vector2(x, -y);
        }

        return result;
    }


    private void CreateObject(Vector3[] vertices, Vector2[] uvs, int[] triangles) {
        //GameObject newObject = Instantiate(newGameObjectPrefab);
        GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
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

    private void CreateRigidObject(Vector3[] vertices, Vector2[] uvs, int[] triangles) {
        //GameObject newObject = Instantiate(newGameObjectPrefab);
        GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
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

    private Vector2 _position = Vector2.zero;
    public Vector2 Position {
        get { return _position; }
    }
    private int _index = 0;
    public int Index {
        get { return _index; }
    }

    private AttVertex _preAttVertex = null;
    public AttVertex PreAttVertex {
        set { _preAttVertex = value; }
        get { return _preAttVertex; }
    }

    private List<AttVertex> _nextAttVertex = new List<AttVertex>();
    public List<AttVertex> NextAttVertex {
        get { return _nextAttVertex; }
    }

    private int _horizontalAdjacentEdge = 0;

    public int HorizontalAdjacentEdge {
        set { _horizontalAdjacentEdge = value; }
        get { return _horizontalAdjacentEdge; }
    }

    private List<int> _helperVertexIndexList = new List<int>();
    public List<int> HelperVertexIndexList {
        get { return _helperVertexIndexList; }
    }
    private int _assort = 0;
    public int Assort {
        get { return _assort; }
    }

    public AttVertex(Vector2 position, int index) {
        this._position = position;
        this._index = index;
    }
    public AttVertex(AttVertex attVertex) {
        this._position = attVertex.Position;
        this._index = attVertex.Index;
        this._preAttVertex = attVertex.PreAttVertex;
        foreach(AttVertex nextAV in attVertex.NextAttVertex)
        this._nextAttVertex.Add(nextAV);
    }

    public void AddNextAttVertex(AttVertex nextVertexIndex) {
        this._nextAttVertex.Add(nextVertexIndex);
    }


    public void AddHelperVertexIndex(int helperVertexIndex) {
        this._helperVertexIndexList.Add(helperVertexIndex);
    }

    public int LatestHeloperVertexIndex() {
        return this._helperVertexIndexList[_helperVertexIndexList.Count - 1];
    }
    public void RemoveLatestHeloperVertexIndex() {
        this._helperVertexIndexList.RemoveAt(_helperVertexIndexList.Count - 1);
    }


    // この頂点を始点とする辺に対して、始点と終点のうちY座標の大きい方を補助頂点として設定する。
    public void InitializeHelperVertexIndex() {
        int helperVertexIndex = 0;
        // 頂点座標の取得
        AttVertex StartAttVertex = this;
        AttVertex EndAttVertex = this.NextAttVertex[0];
        if (StartAttVertex.Position.y > EndAttVertex. Position.y) {
            helperVertexIndex = StartAttVertex.Index;
        } else {
            helperVertexIndex = EndAttVertex.Index;
        }
        this._helperVertexIndexList = new List<int> { helperVertexIndex };
    }

    // 頂点の情報の詳細分類 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
    public void DecomposeVertex() {
        // 頂点座標の取得
        Vector2 preAVertexPos = this.PreAttVertex.Position;
        Vector2 nextAVertexPos = this.NextAttVertex[0].Position;
        // 外積
        DoubleVector2 preToNow = new DoubleVector2(this.Position - preAVertexPos);
        DoubleVector2 nowToNext = new DoubleVector2(nextAVertexPos - this.Position);
        double crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
        // 判別
        // Debug.Log(preToNow.x.ToString() + preToNow.y.ToString());
        // Debug.Log(nowToNext.x.ToString() + nowToNext.y.ToString());
        // Debug.Log(crossProduct);
        int assortVertex = 0; // 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
        if ((crossProduct < 0) && (this.Position.y >= preAVertexPos.y) && (this.Position.y > nextAVertexPos.y)) {
            assortVertex = 1; // StartVertex
        } else if ((crossProduct < 0) && (this.Position.y <= preAVertexPos.y) && (this.Position.y < nextAVertexPos.y)) {
            assortVertex = 2; // EndVertexList
        } else if ((crossProduct > 0) && (this.Position.y >= preAVertexPos.y) && (this.Position.y > nextAVertexPos.y)) {
            assortVertex = 3; // SplitVertex 
        } else if ((crossProduct > 0) && (this.Position.y <= preAVertexPos.y) && (this.Position.y < nextAVertexPos.y)) {
            assortVertex = 4; // MergeVertexList
        } else {
            assortVertex = 0; // 一般のVertex
        }
        this._assort = assortVertex;
    }

    public bool EdgeDirectionUPorDown() {
        return this.Position.y < NextAttVertex[0].Position.y;
    }


    public AttVertex MinAngleNextVertex(Vector2 prePosition) {
        double minAngle = Double.MaxValue;
        AttVertex minNextAttVertex = this.NextAttVertex[0];
        // 頂点座標の取得
        Vector2 preVertex = prePosition;
        Vector2 nowVertex = this.Position;
        foreach (AttVertex nextAttVertex in this.NextAttVertex) {
            Vector2 nextVertex = nextAttVertex.Position;
            // 外積
            DoubleVector2 preToNow = new DoubleVector2(nowVertex - preVertex);
            DoubleVector2 nowToNext = new DoubleVector2(nextVertex - nowVertex);
            double crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
            // 内積を計算
            double dotProduct = DoubleVector2.Dot(preToNow, nowToNext);
            // 大きさを計算
            double magnitudeA = preToNow.magnitude;
            double magnitudeB = nowToNext.magnitude;
            // 角度を計算
            double cosTheta = dotProduct / (magnitudeA * magnitudeB);
            double thetaRadians = Math.Acos(cosTheta);
            double angle = thetaRadians * (180 / Math.PI) * (crossProduct < 0 ? -1 : 1);
            angle += 180;
            // Debug.Log("Angle = " + angle);
            // 角度が最小のベクトルを記録
            if (angle > 0 && angle < minAngle) {
                minAngle = angle;
                minNextAttVertex = nextAttVertex;
            }
        }
        return minNextAttVertex;
    }
}
