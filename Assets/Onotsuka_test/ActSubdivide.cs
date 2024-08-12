using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        List<int>      irrelevantTriangles = new List<int>();
        List<Vector3>  newVerticesList     = new List<Vector3>();
        List<AttIndex> subdivideAttLList   = new List<AttIndex>();
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
        List<int> idnenxList       = new List<int>();
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
                (bool rtlf, int vertexIndex1, Vector3 lonelyVertex, int vertexIndex2, Vector3 startPairVertex, int vertexIndex3, Vector3 lastPairVertex) = SortIndex(targetTriangles[i], vertexTruthValue1, targetVertices[targetTriangles[i]], targetTriangles[i + 1], vertexTruthValue2, targetVertices[targetTriangles[i + 1]], targetTriangles[i + 1], vertexTruthValue3, targetVertices[targetTriangles[i + 2]]);
                (Vector3 newStartPairVertex, Vector3 newLastPairVertex) = GenerateNewVertex(cutter, lonelyVertex, startPairVertex, lastPairVertex);

                newVerticesList.Add(newStartPairVertex);
                newVerticesList.Add(newLastPairVertex);

                AttIndex sal = new AttIndex();
                sal.IndexList = new int[] { vertexIndex1, vertexIndex2, vertexIndex3 };
                sal.VertexList = new Vector3[] { lonelyVertex, startPairVertex, lastPairVertex };
                subdivideAttLList.Add(sal);

            }
            // taro
            // 対象の三角形ポリゴンの頂点すべてが右側にある場合
            // if (vertexTruthValue1 && vertexTruthValue2 && vertexTruthValue3) {
            //     for (int k = 0; k < 3; k++) {
            //         rightVertices.Add(targetVertices[targetTriangles[i + k]]);
            //         rightUVs.Add(targetUVs[targetTriangles[i + k]]);
            //         rightTriangles.Add(rightVertices.Count - 1);
            //     }
            //     rightNormals.Add(targetNormals[targetTriangles[j]]);
            // }
            // // 対象の三角形ポリゴンの頂点すべてが左側にある場合
            // else if (!vertexTruthValue1 && !vertexTruthValue2 && !vertexTruthValue3) {
            //     for (int k = 0; k < 3; k++) {
            //         leftVertices.Add(targetVertices[targetTriangles[i + k]]);
            //         leftUVs.Add(targetUVs[targetTriangles[i + k]]);
            //         leftTriangles.Add(leftVertices.Count - 1);
            //     }
            //     leftNormals.Add(targetNormals[targetTriangles[j]]);
            // }
            // // 対象の三角形ポリゴンの頂点が左右にまたがっている場合
            // else {
            //     (bool rtlf, Vector3 lonelyVertex, Vector3 startPairVertex, Vector3 lastPairVertex) = SortIndex(vertexTruthValue1, vertexTruthValue2, vertexTruthValue3, targetVertices[targetTriangles[i]], targetVertices[targetTriangles[i + 1]], targetVertices[targetTriangles[i + 2]]);

            //     // 右側の頂点処理
            //     if (rtlf) {
            //         (Vector3 newStartPairVertex, Vector3 newLastPairVertex) = GenerateNewVertex(cutter, lonelyVertex, startPairVertex, lastPairVertex);
            //         // 孤独な頂点側のポリゴンを生成
            //         rightVertices.Add(lonelyVertex);
            //         rightUVs.Add(targetUVs[targetTriangles[i]]);
            //         rightTriangles.Add(rightVertices.Count - 1);
            //         rightVertices.Add(newStartPairVertex);
            //         rightUVs.Add(targetUVs[targetTriangles[i + 1]]);
            //         rightTriangles.Add(rightVertices.Count - 1);
            //         rightVertices.Add(newLastPairVertex);
            //         rightUVs.Add(targetUVs[targetTriangles[i + 2]]);
            //         rightTriangles.Add(rightVertices.Count - 1);
            //         rightNormals.Add(targetNormals[targetTriangles[j]]);
            //         // ペア頂点側のポリゴンを生成
            //     }
            //     // 左側の頂点処理
            //     else {
            //         (Vector3 newStartPairVertex, Vector3 newLastPairVertex) = GenerateNewVertex(cutter, lonelyVertex, startPairVertex, lastPairVertex);
            //         // 孤独な頂点側のポリゴンを生成
            //         leftVertices.Add(lonelyVertex);
            //         leftUVs.Add(targetUVs[targetTriangles[i]]);
            //         leftTriangles.Add(leftVertices.Count - 1);
            //         leftVertices.Add(newStartPairVertex);
            //         leftUVs.Add(targetUVs[targetTriangles[i + 1]]);
            //         leftTriangles.Add(leftVertices.Count - 1);
            //         leftVertices.Add(newLastPairVertex);
            //         leftUVs.Add(targetUVs[targetTriangles[i + 2]]);
            //         leftTriangles.Add(leftVertices.Count - 1);
            //         leftNormals.Add(targetNormals[targetTriangles[j]]);
            //         // ペア頂点側のポリゴンを生成
                    
            //     }
            // }
        }
        // 整理する中で作成した新頂点の重複を削除し
        

        // Debug
        // for (int i = 0; i < rightTriangles.Count; i++) {
        //     Debug.Log("Triangle index " + i + ": " + rightTriangles[i]);
        // }
        // for (int i = 0; i < rightVertices.Count; i++) {
        //     Debug.Log("Vertex index " + i + ": " + (rightVertices[i] + new Vector3(0.5f,0.5f,0.5f)));
        // }
        // for (int i = 0; i < rightUVs.Count; i++) {
        //     Debug.Log("uv index " + i + ": " + rightUVs[i] );
        // }
        // for (int i = 0; i < leftTriangles.Count; i++) {
        //     Debug.Log("Triangle index " + i + ": " + leftTriangles[i]);
        // }
        // for (int i = 0; i < leftVertices.Count; i++) {
        //     Debug.Log("Vertex index " + i + ": " + (leftVertices[i] + new Vector3(0.5f, 0.5f, 0.5f)));
        // }
        // for (int i = 0; i < leftUVs.Count; i++)
        // {
        //     Debug.Log("uv index " + i + ": " + leftUVs[i]);
        // }

        CreateObject(leftVertices.ToArray(), leftUVs.ToArray(), leftTriangles.ToArray());
        CreateObject(rightVertices.ToArray(), rightUVs.ToArray(), rightTriangles.ToArray());
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

    // ポリゴンの切断辺の両端に頂点を生成する
    (Vector3 newStartPairVertex, Vector3 newLastPairVertex) GenerateNewVertex(Plane plane, Vector3 lonelyVertex, Vector3 startPairVertex, Vector3 lastPairVertex) {
        Ray ray1 = new Ray(lonelyVertex, startPairVertex - lonelyVertex);
        Ray ray2 = new Ray(lonelyVertex, lastPairVertex - lonelyVertex);
        float distance1 = 0.0f;
        plane.Raycast(ray1, out distance1);
        Vector3 newStartPairVertex = ray1.GetPoint(distance1);
        float distance2 = 0.0f;
        plane.Raycast(ray2, out distance2);
        Vector3 newLastPairVertex = ray2.GetPoint(distance2);
        return (newStartPairVertex, newLastPairVertex);
    }

    private void CreateObject(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
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
public class AttIndex
{
    private int _id;
    public int Id {
        get { return _id; }
        set { _id = value; }
    }
    private int[] _indexList = new int[3];
    public int[] IndexList {
        get { return _indexList; }
        set { _indexList = value; }
    }
    private Vector3[] _vertexList = new Vector3[3];
    public Vector3[] VertexList {
        get { return _vertexList; }
        set { _vertexList = value; }
    }
    private Vector2 _uvList;
    public Vector2 UvList {
        get { return _uvList; }
        set { _uvList = value; }
    }
    private int _indexListStartPair;
    public int IndexListStartPair {
        get { return _indexListStartPair; }
        set { _indexListStartPair = value; }
    }
    private int _indexListLastPair;
    public int IndexListLastPair {
        get { return _indexListLastPair; }
        set { _indexListLastPair = value; }
    }
}