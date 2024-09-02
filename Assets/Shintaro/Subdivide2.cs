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
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

// 切断対象オブジェクトの参照

// Mesh.positions Mesh.normal Mesh.triangle Mesh.uv を取得

// 参照したオブジェクトのメッシュのすべての頂点に対して，無限平面のどちらにあるかを判定する

// 左・右判定された頂点を保持する 

// 左右のばらけているメッシュに対して，新たな頂点を生成する

// すべての頂点に対してポリゴンを形成する

// 切断面の定義，新しいマテリアルの適用
public class Subdivide2 : MonoBehaviour {
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

        /*
        //　五角系In五角系のテストケース
        newVerticesList = new List<Vector3> {
            new Vector3(0, 2, 0),
            new Vector3(0, 0.618f, 1.902f),
            new Vector3(0, -1.618f, 1.176f),
            new Vector3(0, -1.618f, -1.176f),
            new Vector3(0, 0.618f, -1.902f),
            new Vector3(0, 1, 0),
            new Vector3(0, 0.309f, 0.951f),
            new Vector3(0, -0.809f, 0.588f),
            new Vector3(0, -0.809f, -0.588f),
            new Vector3(0, 0.309f, -0.951f)
        };
        vertexSetList = new List<int[]> {
            new int[]{ targetVerticesLength+0, targetVerticesLength + 1 },
            new int[]{ targetVerticesLength + 1,targetVerticesLength + 2 },
            new int[]{ targetVerticesLength + 2,targetVerticesLength + 3 },
            new int[]{ targetVerticesLength + 3,targetVerticesLength + 4 },
            new int[]{ targetVerticesLength + 4,targetVerticesLength + 0 },
            new int[]{ targetVerticesLength + 9,targetVerticesLength + 8 },
            new int[]{ targetVerticesLength + 8,targetVerticesLength + 7 },
            new int[]{ targetVerticesLength + 7,targetVerticesLength + 6 },
            new int[]{ targetVerticesLength + 6,targetVerticesLength + 5 },
            new int[]{ targetVerticesLength + 5,targetVerticesLength + 9 }
        };
        */

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




    // ある頂点に対して水平方向右に隣接する辺を求める．
    private Edge SearchHorizontallyAdjacentEdge(Node vertex, List<Edge> edgeList) {
        Edge horizontallyAdjacentEdge = null;
        // 現在の見ている頂点の座標
        Vector2 targetVertexPos = vertex.Position;
        // 一番近い辺のx座標を初期化
        float nearestCrossPointX = Mathf.Infinity;
        // 各辺について水平線に交差しているかを判定する
        foreach (Edge edge in edgeList) {
            // 現在見ている辺の頂点
            Vector2 startVertexPos = edge.Source.Position;
            Vector2 endVertexPos = edge.Target.Position;
            // startVertex と endVertex の間に targetVertex があるかどうか？ つまり交差してるかってこと
            if ((startVertexPos.y < targetVertexPos.y && endVertexPos.y >= targetVertexPos.y) || (startVertexPos.y >= targetVertexPos.y && endVertexPos.y < targetVertexPos.y)) {
                // 水平線と辺の交点を求める
                float y = targetVertexPos.y;
                //  x = (y - y1)(x2 - x1) / (y2 - y1) + x1;
                float x = (y - startVertexPos.y) * (endVertexPos.x - startVertexPos.x) / (endVertexPos.y - startVertexPos.y) + startVertexPos.x;
                Vector2 crossPoint = new Vector2(x, y);
                // Debug.Log(targetAttVertex.Index.ToString() + targetVertex.ToString() +" -> "+ attVertex.Index.ToString() + startVertex.ToString() + attVertex.NextAttVertex[0].Index.ToString() + endVertex.ToString() + " 交点="+ crossPoint);
                // それは右ですか？しかも一番近いですか？
                if ((crossPoint.x < nearestCrossPointX) && (crossPoint.x > targetVertexPos.x)) {
                    // Debug.Log(crossPoint.x.ToString() + " < " + nearestCrossPointX.ToString());
                    horizontallyAdjacentEdge = edge;
                    nearestCrossPointX = crossPoint.x;
                    // Debug.Log(attVertex.Index.ToString() + attVertex.NextAttVertex[0].Index.ToString() + " 交点=" + crossPoint);
                }
            }
        }
        return horizontallyAdjacentEdge;
    }



    // 辺をたどって、分割された多角形を形成する辺グループに仕分けする
    private List<List<Edge>> EdgeGrouping(List<Edge> edgeList)
    {
        // 未処理Edgeの集合 // 集合だとランダムな要素の取得ができないのでリストする
        // HashSet<Edge> remainingEdgeSet = new HashSet<Edge>();
        List<Edge> remainingEdgeSet = new List<Edge>();
        foreach (Edge edge in edgeList) {
            remainingEdgeSet.Add(edge);
        }
        Debug.Log("remainingEdgeSet: " + string.Join(", ", remainingEdgeSet.Select(a => a.ToString())) + ")");

        // 多角形を形成する辺グループのリスト
        List<List<Edge>> edgeGroupList = new List<List<Edge>>();

        // まだ処理されていない辺が残っている場合、グループ化を続ける
        while (remainingEdgeSet.Count > 0)
        {
            List<Edge> edgeGroup = new List<Edge>();
            // 頂点グループの最初のEdgeの開始点と終点を取得
            Edge nowEdge = remainingEdgeSet[0];
            Edge startEdge = nowEdge;
            // 現在のEdgeを辺グループに追加、
            edgeGroup.Add(nowEdge);
            // 現在のEdgeを削除し、次のEdgeへ移動
            remainingEdgeSet.Remove(nowEdge);
            nowEdge = nowEdge.MinAngleEdge();
            while (startEdge != nowEdge)
            {
                // 現在のEdgeを辺グループに追加、
                edgeGroup.Add(nowEdge);
                // 次のEdgeへ移動し、現在のEdgeを削除
                remainingEdgeSet.Remove(nowEdge);
                nowEdge = nowEdge.MinAngleEdge();
                // Debug.Log("while: " + startVertexIndex.ToString() + "!=" + endVertexIndex.ToString());
            }

            // 終点が開始点に戻ってきたら終わり
            edgeGroupList.Add(edgeGroup);
            // Debug.Log("remainingEdgeSet: " + string.Join(", ", remainingEdgeSet.Select(a => a.ToString())));
            // ebug.Log("Count: " + remainingEdgeSet.Count);
        }

        // 全ての辺が処理された場合、結果を返す
        return edgeGroupList;
    }


    // 辺を作成し頂点をつなぐ
    private Edge CreateEdge(Node startVertex, Node endVertex)
    {
        Edge newEdge = new Edge(startVertex, endVertex);
        startVertex.AddOutComingEdge(newEdge); 
        endVertex.AddInComingEdge(newEdge);
        Debug.Log("Add Edge" + startVertex.Index.ToString() + endVertex.Index.ToString());
        return newEdge;
    }


    // 必ずしも凸でない穴のあいた図形を単調多角形に分割する：一般点の場合の処理
    private void HandleRegularVertex(Node targetVertexNode, List<Edge> edgeList)
    {
        if (!targetVertexNode.OutComingEdgeList[0].DirectionUPorDown())
        {
            // 下向きの辺を持つ一般点の頂点の場合 // 右回りの時、辺が下向きの場合は右側（X軸方向負）が内部となる。
            // Debug.Log("下向き");
            // ひとつ前の辺の最新のヘルパー頂点を調べる。
            Edge inComingEdge = targetVertexNode.InComingEdgeList[0];
            // Debug.Log("inComingEdge: " + inComingEdge.ToString());
            Node inComingEdgeHelper = inComingEdge.LatestHeloperVertex();
            // Debug.Log("inComingEdgeHelper.Index: " + inComingEdgeHelper.Index.ToString());
            // ヘルパー頂点が統合点なら対角線を双方向に引き、ヘルパー頂点を消す。
            if (inComingEdgeHelper.Assort == 4)
            {
                edgeList.Add(CreateEdge(inComingEdgeHelper, targetVertexNode));
                edgeList.Add(CreateEdge(targetVertexNode, inComingEdgeHelper));
                inComingEdge.RemoveLatestHeloperVertex();
            }
        }
        else if (targetVertexNode.OutComingEdgeList[0].DirectionUPorDown())
        {
            // 上向きの辺を持つ一般点の頂点の場合 // 右回りの時、辺が上向きの場合は左側（X軸方向正）が内部となる。
            // Debug.Log("上向き");
            // 水平隣接辺の最新のヘルパー頂点を調べる。
            Edge horizontalAdjacentEdge = targetVertexNode.HorizontalAdjacentEdge;
            // Debug.Log("horizontalAdjacentEdge: " + horizontalAdjacentEdge.ToString());
            Node horizontalAdjacentEdgeHelper = horizontalAdjacentEdge.LatestHeloperVertex();
            // Debug.Log("horizontalAdjacentEdgeHelper.Index: " + haeAttVertexHelper.Index.ToString());
            // ヘルパー頂点が統合点なら対角線を引き、ヘルパー頂点を消す。
            if (horizontalAdjacentEdgeHelper.Assort == 4)
            {
                edgeList.Add(CreateEdge(horizontalAdjacentEdgeHelper, targetVertexNode));
                edgeList.Add(CreateEdge(targetVertexNode, horizontalAdjacentEdgeHelper));
                horizontalAdjacentEdge.RemoveLatestHeloperVertex();
            }
        }
    }


    // 必ずしも凸でない穴のあいた図形を単調多角形に分割する：分割点の場合の処理
    private void HandleSplitVertex(Node targetVertexNode, List<Edge> edgeList)
    {
        // 水平隣接辺の最新のヘルパー頂点を調べる。
        Edge horizontalAdjacentEdge = targetVertexNode.HorizontalAdjacentEdge;
        // Debug.Log("horizontalAdjacentEdge: " + horizontalAdjacentEdge.ToString());
        Node horizontalAdjacentEdgeHelper = horizontalAdjacentEdge.LatestHeloperVertex();
        // Debug.Log("horizontalAdjacentEdgeHelper.Index: " + haeAttVertexHelper.Index.ToString());
        // ヘルパー頂点と対角線を結ぶ
        edgeList.Add(CreateEdge(horizontalAdjacentEdgeHelper, targetVertexNode));
        edgeList.Add(CreateEdge(targetVertexNode, horizontalAdjacentEdgeHelper));
        // ヘルパー頂点が統合点なら消す
        if (horizontalAdjacentEdgeHelper.Assort == 4)
        {
            horizontalAdjacentEdge.RemoveLatestHeloperVertex();
        }
    }

    // 必ずしも凸でない穴のあいた図形を単調多角形に分割する：統合点の場合の処理
    private void HandleMergeVertex(Node targetVertexNode)
    {
        // 水平隣接辺のヘルパー頂点リストに、自分を追加
        Edge horizontalAdjacentEdge = targetVertexNode.HorizontalAdjacentEdge;
        // Debug.Log("horizontalAdjacentEdge: " + horizontalAdjacentEdge.ToString());
        horizontalAdjacentEdge.AddHelperVertex(targetVertexNode);
        // Debug.Log("horizontalAdjacentEdgeHelper: " + string.Join(", ", horizontalAdjacentEdge.HelperVertexList.Select(a => a.ToString())));
    }

    // 必ずしも凸でない穴のあいた図形を単調多角形に分割する

    private List<List<Edge>> DivideComplexGeometryToMonotoneGeometry(Node[] vertexNodeAry, List<Edge> edgeList) {
        // Debug.Log(string.Join(", ", verticesAry2D.Select(obj => obj.ToString())));
        // (0.00, 0.50, 0.50), (0.00, 0.00, 0.50), (0.00, -0.50, 0.50), (0.00, 0.50, -0.50), (0.00, 0.50, 0.00), (0.00, -0.50, -0.50), (0.00, 0.00, -0.50), (0.00, -0.50, 0.00)
        // (0.50, -0.50),      (0.50, 0.00),       (0.50, 0.50),         (-0.50, -0.50),     (0.00, -0.50),      (-0.50, 0.50),        (-0.50, 0.00),       (0.00, 0.50)

        foreach (Node vertexNode in vertexNodeAry) {
            // 水平隣接辺を取得
            vertexNode.HorizontalAdjacentEdge = SearchHorizontallyAdjacentEdge(vertexNode, edgeList);
            // 頂点を分類
            vertexNode.DecomposeVertex();
        }

        foreach (Edge edge in edgeList) {
            // ヘルパー頂点を初期化
            edge.InitializeHelperVertex();
        }

        Debug.Log("HorizontalAdjacentEdge: " + string.Join(", ", vertexNodeAry.Select(obj => obj.HorizontalAdjacentEdge != null ? obj.HorizontalAdjacentEdge.ToString() : "null" )));
        Debug.Log("HelperVertexList: " + string.Join(", ", edgeList.Select(a => "(" + string.Join(", ", a.HelperVertexList.Select(b => b.Index.ToString())) + ")")));
        Debug.Log("Assort: " + string.Join(", ", vertexNodeAry.Select(obj => obj.Assort.ToString())));


        // Y座標でソートした頂点番号を作成する
        int[] sortedVertxIndexAry = new int[vertexNodeAry.Length];

        // float[] keys = new float[attVertexAry.Length];
        for (int i = 0; i < vertexNodeAry.Length; i++) {
            sortedVertxIndexAry[i] = i;
        //    keys[i] = attVertexAry[i].Position.y * 1_000_000 + attVertexAry[i].Position.x;
        }
        // Array.Sort(keys, sortedVertxIndexAry);
        // Debug.Log("sortedVertxIndexAry: " + string.Join(", ", sortedVertxIndexAry.Select(obj => attVertexAry[obj].Position.y.ToString())));
        // CompareToでソート
        Array.Sort(sortedVertxIndexAry, (a, b) => {
            float a_x = vertexNodeAry[a].Position.x;
            float a_y = vertexNodeAry[a].Position.y;
            float b_x = vertexNodeAry[b].Position.x;
            float b_y = vertexNodeAry[b].Position.y;
            if (a_y != b_y)
                return b_y.CompareTo(a_y); // Yの降順
            return b_x.CompareTo(a_x);     // Xの降順
        });
        Debug.Log("sortedVertxIndexAry: " + string.Join(", ", sortedVertxIndexAry.Select(obj => vertexNodeAry[obj].Index.ToString())));

        // ここからが本番
        // y座標が一番大きい頂点から順に処理
        foreach (int vertexIndex in sortedVertxIndexAry) {
            // 今見る頂点
            Node targetVertexNode = vertexNodeAry[vertexIndex];
            Debug.Log("targetVertexNode: " + targetVertexNode.Index.ToString());

            // 頂点の分類  0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
            // Debug.Log("VertexAssort: " + targetVertexNode.Assort.ToString());
            if (targetVertexNode.Assort == 0) {
                // 一般点の場合の処理
                HandleRegularVertex(targetVertexNode, edgeList);
            }
            else if (targetVertexNode.Assort == 3) {
                // 分割点の場合の処理
                HandleSplitVertex(targetVertexNode, edgeList);
            } else if (targetVertexNode.Assort == 4) {
                // 統合点の場合の処理
                HandleMergeVertex(targetVertexNode);
            }
        }

        // 単調多角形を形成する辺リスト
        List<List<Edge>> MonotoneGeometryEdgeGroupList = EdgeGrouping(edgeList);

        foreach (List<Edge> MonotoneGeometryEdgeGroup in MonotoneGeometryEdgeGroupList) {
            Debug.Log("OutEdgeGroup " + string.Join(", ", MonotoneGeometryEdgeGroup.Select(obj => obj.ToString())));
        }
        return MonotoneGeometryEdgeGroupList;
    }



    // 単調多角形を三角形に分割する
    private List<int[]> DivideMonotoneGeometryToTriangle(List<Edge> edgeList)
    {
        // CompareToでソート
        edgeList.Sort((a, b) => {
            if (a.Source.Position.y != b.Source.Position.y)
                return b.Source.Position.y.CompareTo(a.Source.Position.y); // Yの降順
            return b.Source.Position.x.CompareTo(a.Source.Position.x);     // Xの降順
        });

        List<Node> sortedVertexList = new List<Node>();

        List<Node> leftChainVertexSet = new List<Node>();
        List<Node> rightChainVertexSet = new List<Node>();

        foreach (Edge edge in edgeList) {
            sortedVertexList.Add(edge.Source);
            if (edge.DirectionUPorDown()) {
                leftChainVertexSet.Add(edge.Source);
            } else {
                rightChainVertexSet.Add(edge.Source);
            };
        }
        Debug.Log("sortedVertexList " + string.Join(", ", sortedVertexList.Select(obj => obj.Index.ToString())));

        // y座標が一番大きい辺から順に処理
        Stack<Node> vertexStack = new Stack<Node>();

        List<Edge> newEdgeList = new List<Edge>();

        Edge nowEdge = edgeList[0];
        bool stackSideLTRF = nowEdge.DirectionUPorDown();
        vertexStack.Push(sortedVertexList[0]);
        vertexStack.Push(sortedVertexList[1]);
        stackSideLTRF = leftChainVertexSet.Contains(sortedVertexList[1]);

        for (int i = 2; i < sortedVertexList.Count - 1; i++) {

            Debug.Log("stackSideLTRF " + stackSideLTRF.ToString());
            Node targetVertex = sortedVertexList[i];
            Debug.Log("targetVertex " + targetVertex.Index.ToString());
            bool targetVertexSide = leftChainVertexSet.Contains(targetVertex);
            Debug.Log("targetVertexSide " + (targetVertexSide));
            Debug.Log("vertexStack " + string.Join(", ", vertexStack.Select(obj => obj.Index.ToString())));
            // ujとSの一番上の頂点が異なる側チェイン上にある // 排他的OR
            if (stackSideLTRF ^ targetVertexSide) {
                // Sからすべての頂点を取り出す  
                while (vertexStack.Count > 0) {
                    Node popVertex = vertexStack.Pop();
                    if (vertexStack.Count == 0) break;
                    Edge newEdge = CreateEdge(popVertex, targetVertex);
                    Edge newEdgeR = CreateEdge(targetVertex, popVertex);
                    newEdgeList.Add(newEdge);
                    newEdgeList.Add(newEdgeR);
                }
                vertexStack.Push(sortedVertexList[i - 1]);
                vertexStack.Push(sortedVertexList[i]);
                stackSideLTRF = leftChainVertexSet.Contains(sortedVertexList[i]);
            } else {
                Node popVertex1 = vertexStack.Pop(); // すでに辺がある
                // ujとSの一番上の頂点が同じ側チェイン上にある 
                while (vertexStack.Count > 0) { 
                    // Sから1つ頂点を取り出す
                    Node popVertex2 = vertexStack.Pop();
                    // 対角線が図形の内部に収まるか？
                    DoubleVector2 a = new DoubleVector2(targetVertex.Position);
                    DoubleVector2 b = new DoubleVector2(popVertex1.Position);
                    DoubleVector2 c = new DoubleVector2(popVertex2.Position);
                    // 外積を用いた判定法
                    double crossProduct = (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);

                    if (stackSideLTRF ^ (crossProduct > 0)) {
                        vertexStack.Push(popVertex2);
                        break;
                    }
                    // 引けるとき
                    Edge newEdge = CreateEdge(popVertex2, targetVertex);
                    Edge newEdgeR = CreateEdge(targetVertex, popVertex2);
                    newEdgeList.Add(newEdge);
                    newEdgeList.Add(newEdgeR);
                    // ずらす
                    popVertex1 = popVertex2;
                }
                vertexStack.Push(popVertex1);
                vertexStack.Push(targetVertex);
                stackSideLTRF = leftChainVertexSet.Contains(sortedVertexList[i]);
            }
        }
        // 最後の頂点はよくわからん
        Debug.Log("targetVertex " + sortedVertexList[sortedVertexList.Count - 1].Index.ToString());
        vertexStack.Pop();
        while (vertexStack.Count > 0)
        {
            Node popVertex = vertexStack.Pop();
            if (vertexStack.Count == 0) break;
            Edge newEdge = CreateEdge(popVertex, sortedVertexList[sortedVertexList.Count-1]);
            Edge newEdgeR = CreateEdge(sortedVertexList[sortedVertexList.Count-1], popVertex);
            newEdgeList.Add(newEdge);
            newEdgeList.Add(newEdgeR);
        }

        edgeList.AddRange(newEdgeList);

        Debug.Log("newEdgeList " + string.Join(", ", newEdgeList.Select(obj => obj.ToString())));
        List<int[]> allTriangleList = new List<int[]>();

        List<List<Edge>> triangleGroupList = EdgeGrouping(edgeList);

        foreach (List<Edge> triangleGroup in triangleGroupList) {
            Debug.Log("OutEdgeGroup " + string.Join(", ", triangleGroup.Select(obj => obj.ToString())));
        }

        foreach (List<Edge> triangleGroup in triangleGroupList) {
            int[] triangleVertexAry = new int[3] {
                triangleGroup[0].Source.Index,
                triangleGroup[1].Source.Index,
                triangleGroup[2].Source.Index
            };
            allTriangleList.Add(triangleVertexAry);
        }

        return allTriangleList;
    }



    private List<int[]> DivideComplexGeometryToTriangle(List<int[]> vertexSetList, int offsetVertexIndex, Vector2[] verticesAry2D) {

        Node[] vertexNodeAry = new Node[verticesAry2D.Length];
        for (int i = 0; i < verticesAry2D.Length; i++) {
            vertexNodeAry[i] = new Node(i+ offsetVertexIndex, verticesAry2D[i]);
        }
        
        List<Edge> EdgeList = new List<Edge>();
        foreach (int[] vertexSet in vertexSetList) {
            Node startVertex = vertexNodeAry[vertexSet[0]- offsetVertexIndex];
            Node endVertex = vertexNodeAry[vertexSet[1] - offsetVertexIndex];
            Edge newEdge = CreateEdge(startVertex, endVertex);
            EdgeList.Add(newEdge);
        }

        foreach (Node vertexNode in vertexNodeAry)
        {
            Debug.Log(vertexNode.Print());
        }

        List<List<Edge>> monotoneGeometryEdgeGroupList = DivideComplexGeometryToMonotoneGeometry(vertexNodeAry, EdgeList);

        List<int[]> allTriangleList = new List<int[]>();

        foreach (List<Edge> monotoneGeometryEdgeGroup in monotoneGeometryEdgeGroupList) {
            allTriangleList.AddRange( DivideMonotoneGeometryToTriangle(monotoneGeometryEdgeGroup));
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


public class Edge
{

    private Node _source = null;
    public Node Source {
        get { return _source; }
    }

    private Node _target = null;
    public Node Target {
        get { return _target; }
    }

    private List<Node> _helperVertexList = null;
    public List<Node> HelperVertexList {
        get { return _helperVertexList; }
    }

    public Edge(Node source, Node target) {
        this._source = source;
        this._target = target;
    }

    public void AddHelperVertex(Node helperVertex) {
        this._helperVertexList.Add(helperVertex);
    }

    public Node LatestHeloperVertex() {
        return this._helperVertexList[_helperVertexList.Count - 1];
    }
    public void RemoveLatestHeloperVertex() {
        this._helperVertexList.RemoveAt(_helperVertexList.Count - 1);
    }


    // この辺に対して、始点と終点のうちY座標の大きい方を補助頂点として設定する。
    public void InitializeHelperVertex() {
        Node helperVertex = null;
        if (this.Source.Position.y > this.Target.Position.y) {
            helperVertex = this.Source;
        } else {
            helperVertex = this.Target;
        }
        this._helperVertexList = new List<Node> { helperVertex };
    }

    public bool DirectionUPorDown() {

        if (this.Source.Position.y != this.Target.Position.y)
            return (this.Source.Position.y < this.Target.Position.y); // Yの降順
        return (this.Source.Position.x < this.Target.Position.x);     // Xの降順
    }

    public DoubleVector2 GetDoubleVector2()
    {
        return new DoubleVector2(this.Target.Position) - new DoubleVector2(this.Source.Position);
    }

    public Edge MinAngleEdge()
    {
        double minAngle = Double.MaxValue;
        Edge minAngleEdge = null;
        // 頂点座標の取得
        DoubleVector2 vectorA = this.GetDoubleVector2();
        foreach (Edge OutComingEdge in this.Target.OutComingEdgeList)
        {
            DoubleVector2 vectorB = OutComingEdge.GetDoubleVector2();
            // 外積
            double crossProduct = DoubleVector2.Cross(vectorA, vectorB);
            // 内積を計算
            double dotProduct = DoubleVector2.Dot(vectorA, vectorB);
            // 大きさを計算
            double magnitudeA = vectorA.magnitude;
            double magnitudeB = vectorB.magnitude;
            // 角度を計算
            double cosTheta = dotProduct / (magnitudeA * magnitudeB);
            double thetaRadians = Math.Acos(cosTheta);
            double angle = thetaRadians * (180 / Math.PI) * (crossProduct < 0 ? -1 : 1);
            angle += 180;
            // Debug.Log("Angle = " + angle);
            // 角度が最小のベクトルを記録
            if (angle > 0 && angle < minAngle)
            {
                minAngle = angle;
                minAngleEdge = OutComingEdge;
            }
        }
        return minAngleEdge;
    }
    public override string ToString()
    {
        return $"({Source.Index}, {Target.Index})";
    }

    public bool JudgeCross(Edge edge)
    {
        DoubleVector2 vectorAB = this.GetDoubleVector2();

        DoubleVector2 vectorAC = new DoubleVector2(edge.Source.Position) -  new DoubleVector2(this.Source.Position);
        DoubleVector2 vectorAD = new DoubleVector2(edge.Target.Position) - new DoubleVector2(this.Source.Position);
        // 外積
        double s = DoubleVector2.Cross(vectorAB, vectorAC);
        double t = DoubleVector2.Cross(vectorAB, vectorAD);
        return (s*t < 0);
    }
}

public class Node
{
    private int _index = 0;
    public int Index {
        get { return _index; }
    }

    private Vector2 _position = Vector2.zero;
    public Vector2 Position {
        get { return _position; }
    }

    
    private List<Edge> _inComingEdgeList = null;
    public List<Edge> InComingEdgeList {
        set { _inComingEdgeList = value; }
        get { return _inComingEdgeList; }
    }

    private List<Edge> _outComingEdgeList = null;
    public List<Edge> OutComingEdgeList {
        set { _outComingEdgeList = value; }
        get { return _outComingEdgeList; }
    }

    private int _assort = 0;
    public int Assort {
        get { return _assort; }
    }

    private Edge _horizontalAdjacentEdge = null;

    public Edge HorizontalAdjacentEdge {
        set { _horizontalAdjacentEdge = value; }
        get { return _horizontalAdjacentEdge; }
    }

    public Node(int index, Vector2 position) {
        this._index = index;
        this._position = position;
        this._inComingEdgeList = new List<Edge>();
        this._outComingEdgeList = new List<Edge>();
    }

    public void AddInComingEdge(Edge inComingEdge) {
        this._inComingEdgeList.Add(inComingEdge);
    }

    public void AddOutComingEdge(Edge outComingEdge) {
        this._outComingEdgeList.Add(outComingEdge);
    }

    // 頂点の分類 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
    public void DecomposeVertex() {
        // 頂点座標の取得
        Vector2 preVertexPos = this.InComingEdgeList[0].Source.Position;
        Vector2 nextVertexPos = this.OutComingEdgeList[0].Target.Position;
        // 外積
        DoubleVector2 preToNow = new DoubleVector2(this.Position - preVertexPos);
        DoubleVector2 nowToNext = new DoubleVector2(nextVertexPos - this.Position);
        double crossProduct = DoubleVector2.Cross(preToNow, nowToNext);
        // 判別
        // Debug.Log(preToNow.ToString());
        // Debug.Log(nowToNext.ToString());
        // Debug.Log(crossProduct);
        int assortVertex = 0; // 0:一般のVertex 1:StartVertex, 2:EndVertexList, 3:SplitVertex, 4:MergeVertexList
        if ((crossProduct < 0) && (this.Position.y >= preVertexPos.y) && (this.Position.y > nextVertexPos.y)) {
            assortVertex = 1; // StartVertex
        }
        else if ((crossProduct < 0) && (this.Position.y <= preVertexPos.y) && (this.Position.y < nextVertexPos.y)) {
            assortVertex = 2; // EndVertexList
        }
        else if ((crossProduct > 0) && (this.Position.y > preVertexPos.y) && (this.Position.y >= nextVertexPos.y)) {
            assortVertex = 3; // SplitVertex 
        }
        else if ((crossProduct > 0) && (this.Position.y < preVertexPos.y) && (this.Position.y <= nextVertexPos.y)) {
            assortVertex = 4; // MergeVertexList
        }
        this._assort = assortVertex;
        // Debug.Log(assortVertex);

    }
    public string Print()
    {
        return $"(Index: {Index}, Pos: {Position.ToString()}, InComingEdge: {InComingEdgeList[0].ToString()}, OutComingEdgeList: {OutComingEdgeList[0].ToString()})";
    }
}
