using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateMaze : MonoBehaviour {
	private const int mazeSize = 21;
	bool[,] map;
	public Material mazeMat;

	void Start() {
		map = new bool[mazeSize, mazeSize];
		mazifyMap();
		displayMap();
		constructMesh();
	}

	void constructMesh() {
		//count number of walls
		int numWalls = 0;
		for (int i = 0; i < mazeSize; ++i) {
			for (int r = 0; r < mazeSize; ++r) {
				if (map[i, r]) {
					++numWalls;
				}
			}
		}

		//prepare mesh components
		MeshFilter mf = gameObject.AddComponent< MeshFilter > ();
		MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
		mr.material = mazeMat;
		Mesh mesh = new Mesh();
		mf.mesh = mesh;

		//init mesh data arrays
		Vector3[] vertices = new Vector3[8*numWalls];
		int[] tri = new int[30 * numWalls];
		Vector3[] normals = new Vector3[8 * numWalls];
		Vector2[] uv = new Vector2[8 * numWalls];

		//generate independent wall pieces
		int curPiece = 0;
		for (int i = 0; i < mazeSize; ++i) {
			for (int r = 0; r < mazeSize; ++r) {
				if (map[i, r]) {
					//construct verts (4 top, then 4 bottom)
					for (int j = 0; j < 2; ++j) {
						vertices[curPiece * 8+4*j] = new Vector3(i, 1-j, r);
						vertices[curPiece * 8 + 1+4*j] = new Vector3(i + 1, 1 - j, r);
						vertices[curPiece * 8 + 2 + 4 * j] = new Vector3(i, 1 - j, r + 1);
						vertices[curPiece * 8 + 3 + 4 * j] = new Vector3(i + 1, 1 - j, r + 1);
					}

					//construct tris
					int[,] pieceNumbers;
					//define vertex orders for each of the 5 wall polys
					pieceNumbers = new int[,] {
						{
							0,1,2,3
						},
						{
							4,5,0,1
						},
						{
							2,3,6,7
						},
						{
							0,2,4,6
						},
						{
							5,7,1,3
						}
					};

					for (int j = 0; j < 5; ++j) {
						tri[curPiece * 30 + 6*j] = curPiece * 8 + pieceNumbers[j,0];
						tri[curPiece * 30 + 1+ 6 * j] = curPiece * 8 + +pieceNumbers[j, 2];
						tri[curPiece * 30 + 2 + 6 * j] = curPiece * 8 + +pieceNumbers[j, 1];
						tri[curPiece * 30 + 3 + 6 * j] = curPiece * 8 + +pieceNumbers[j, 2];
						tri[curPiece * 30 + 4 + 6 * j] = curPiece * 8 + +pieceNumbers[j, 3];
						tri[curPiece * 30 + 5 + 6 * j] = curPiece * 8 + +pieceNumbers[j, 1];
					}

					//construct normals (value here doesn't matter as Unity will recalculate these for us)
					for (int j = 0; j < 2; ++j) {
						normals[curPiece * 8 + 4 * j] = -Vector3.forward;
						normals[curPiece * 8 + 1 + 4 * j] = -Vector3.forward;
						normals[curPiece * 8 + 2 + 4 * j] = -Vector3.forward;
						normals[curPiece * 8 + 3 + 4 * j] = -Vector3.forward;
					}

					//construct uvs
					for (int j = 0; j < 2; ++j) {
						uv[curPiece * 8 + 4*j] = new Vector2(j, j);
						uv[curPiece * 8 + 1 + 4 * j] = new Vector2(1-j, j);
						uv[curPiece * 8 + 2 + 4 * j] = new Vector2(j, 1-j);
						uv[curPiece * 8 + 3 + 4 * j] = new Vector2(1-j, 1-j);
					}

					++curPiece;
				}
			}
		}

		//assign mesh vals to mesh component
		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.normals = normals;
		mesh.uv = uv;

		//recalculate normals
		mf.mesh.RecalculateNormals();
	}

	/**
	 * code adapted from https://en.wikipedia.org/wiki/Maze_generation_algorithm
	 **/
	void mazifyMap(float complexity = .75f, float density = .75f) {
		//Only odd shapes
		Vector2 shape = new Vector2(mazeSize, mazeSize);
		//Adjust complexity and density relative to maze size
		int roundedComplexity = (int)(complexity * (5 * (shape[0] + shape[1])));
		int roundedDensity = (int)(density * ((shape[0] / 2) * (shape[1] / 2)));

		//Fill borders
		for (int i = 0; i < mazeSize; ++i) {
			map[i,0] = true;
			map[i,mazeSize - 1] = true;
			map[0, i] = true;
			map[mazeSize - 1, i] = true;
		}

		//Make aisles
		for (int i = 0; i < roundedDensity; ++i) {
			int x = Mathf.RoundToInt(Random.Range(0, (int)(shape[0]/2))*2);
			int y = Mathf.RoundToInt(Random.Range(0, (int)(shape[1]/2))*2);
			map[x, y] = true;

			for (int j = 0; j < roundedComplexity; ++j) {
				List<Vector2> neighbours = new List<Vector2>();
				if (x > 1) {
					neighbours.Add(new Vector2(x - 2, y));
				}
				if (x < shape[1] - 2) {
					neighbours.Add(new Vector2(x + 2, y));
				}
				if (y > 1) {
					neighbours.Add(new Vector2(x, y - 2));
				}
				if (y < shape[0] - 2) {
					neighbours.Add(new Vector2(x, y + 2));
				}
				
				if (neighbours.Count > 0) {
					int randomIndex = Mathf.RoundToInt(Random.Range(0, neighbours.Count));
					int x_ = (int)neighbours[randomIndex].x;
					int y_ = (int)neighbours[randomIndex].y;
					if (map[x_, y_] == false) {
						map[x_, y_] = true;
						map[x_ + (x - x_) / 2, y_ + (y - y_) / 2] = true;
						x = x_;
						y = y_;
					}
				}
			}
		}
	}

	void displayMap() {
		string mapStr = "";
		for (int i = 0; i < mazeSize; ++i) {
			mapStr += '|';
			for (int r = 0; r < mazeSize; ++r) {
				mapStr += (map[i, r] ? 'o' : '_') + "|";
			}
			if (i != mazeSize - 1) {
				mapStr += '\n';
			}
		}
		Debug.Log(mapStr);
	}
}
