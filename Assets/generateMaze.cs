using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateMaze : MonoBehaviour {
	private const int mazeSize = 9;
	bool[,] map;

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
		gameObject.AddComponent<MeshRenderer>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;

		//init mesh data arrays
		Vector3[] vertices = new Vector3[4*numWalls];
		int[] tri = new int[6 * numWalls];
		Vector3[] normals = new Vector3[4 * numWalls];
		Vector2[] uv = new Vector2[4 * numWalls];

		//generate independent wall pieces
		int curPiece = 0;
		for (int i = 0; i < mazeSize; ++i) {
			for (int r = 0; r < mazeSize; ++r) {
				if (map[i, r]) {
					//construct verts
					vertices[curPiece*4] = new Vector3(i, 0, r);
					vertices[curPiece * 4+1] = new Vector3(i+1, 0, r);
					vertices[curPiece * 4+2] = new Vector3(i, 0, r+1);
					vertices[curPiece * 4+3] = new Vector3(i+1, 0, r+1);

					//construct tris
					tri[curPiece*6] = curPiece * 4;
					tri[curPiece * 6+1] = curPiece * 4+2;
					tri[curPiece * 6+2] = curPiece * 4+1;

					tri[curPiece * 6+3] = curPiece * 4+2;
					tri[curPiece * 6+4] = curPiece * 4+3;
					tri[curPiece * 6+5] = curPiece * 4+1;

					//construct normals
					normals[curPiece*4] = -Vector3.forward;
					normals[curPiece * 4+1] = -Vector3.forward;
					normals[curPiece * 4+2] = -Vector3.forward;
					normals[curPiece * 4+3] = -Vector3.forward;

					//construct uvs
					uv[curPiece*4] = new Vector2(0, 0);
					uv[curPiece * 4+1] = new Vector2(1, 0);
					uv[curPiece * 4+2] = new Vector2(0, 1);
					uv[curPiece * 4+3] = new Vector2(1, 1);

					++curPiece;
				}
			}
		}

		//assign mesh vals to mesh component
		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.normals = normals;
		mesh.uv = uv;
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
