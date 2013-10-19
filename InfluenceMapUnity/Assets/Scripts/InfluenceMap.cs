/*
Copyright (C) 2012 Anomalous Underdog

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Vector2I
{
	public int x;
	public int y;
	public float d;

	public Vector2I(int nx, int ny)
	{
		x = nx;
		y = ny;
		d = 1;
	}

	public Vector2I(int nx, int ny, float nd)
	{
		x = nx;
		y = ny;
		d = nd;
	}
}

public class InfluenceMap : GridData
{
	List<IPropagator> _propagators = new List<IPropagator>();

	float[,] _influences;
	float[,] _influencesBuffer;
	public float Decay { get; set; }
	public float Momentum { get; set; }
	public int Width { get{ return _influences.GetLength(0); } }
	public int Height { get{ return _influences.GetLength(1); } }
	public float GetValue(int x, int y)
	{
		return _influences[x, y];
	}
	
	public InfluenceMap(int size, float decay, float momentum)
	{
		_influences = new float[size, size];
		_influencesBuffer = new float[size, size];
		Decay = decay;
		Momentum = momentum;
	}
	
	public InfluenceMap(int width, int height, float decay, float momentum)
	{
		_influences = new float[width, height];
		_influencesBuffer = new float[width, height];
		Decay = decay;
		Momentum = momentum;
	}
	
	public void SetInfluence(int x, int y, float value)
	{
		if (x < Width && y < Height)
		{
			_influences[x, y] = value;
			_influencesBuffer[x, y] = value;
		}
	}

	public void SetInfluence(Vector2I pos, float value)
	{
		if (pos.x < Width && pos.y < Height)
		{
			_influences[pos.x, pos.y] = value;
			_influencesBuffer[pos.x, pos.y] = value;
		}
	}

	public void RegisterPropagator(IPropagator p)
	{
		_propagators.Add(p);
	}

	public void Propagate()
	{
		UpdatePropagators();
		UpdatePropagation();
		UpdateInfluenceBuffer();
	}

	void UpdatePropagators()
	{
		foreach (IPropagator p in _propagators)
		{
			SetInfluence(p.GridPosition, p.Value);
		}
	}

	void UpdatePropagation()
	{
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx)
		{
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx)
			{
				//Debug.Log("at " + xIdx + ", " + yIdx);
				float maxInf = 0.0f;
				float minInf = 0.0f;
				Vector2I[] neighbors = GetNeighbors(xIdx, yIdx);
				foreach (Vector2I n in neighbors)
				{
					//Debug.Log(n.x + " " + n.y);
					float inf = _influencesBuffer[n.x, n.y] * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf = Mathf.Max(inf, maxInf);
					minInf = Mathf.Min(inf, minInf);
				}
				
				if (Mathf.Abs(minInf) > maxInf)
				{
					_influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], minInf, Momentum);
				}
				else
				{
					_influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], maxInf, Momentum);
				}
			}
		}
	}

	void UpdateInfluenceBuffer()
	{
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx)
		{
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx)
			{
				_influencesBuffer[xIdx, yIdx] = _influences[xIdx, yIdx];
			}
		}
	}
	
	Vector2I[] GetNeighbors(int x, int y)
	{
		List<Vector2I> retVal = new List<Vector2I>();
		
		// as long as not in left edge
		if (x > 0)
		{
			retVal.Add(new Vector2I(x-1, y));
		}

		// as long as not in right edge
		if (x < _influences.GetLength(0)-1)
		{
			retVal.Add(new Vector2I(x+1, y));
		}
		
		// as long as not in bottom edge
		if (y > 0)
		{
			retVal.Add(new Vector2I(x, y-1));
		}

		// as long as not in upper edge
		if (y < _influences.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x, y+1));
		}


		// diagonals

		// as long as not in bottom-left
		if (x > 0 && y > 0)
		{
			retVal.Add(new Vector2I(x-1, y-1, 1.4142f));
		}

		// as long as not in upper-right
		if (x < _influences.GetLength(0)-1 && y < _influences.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x+1, y+1, 1.4142f));
		}

		// as long as not in upper-left
		if (x > 0 && y < _influences.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x-1, y+1, 1.4142f));
		}

		// as long as not in bottom-right
		if (x < _influences.GetLength(0)-1 && y > 0)
		{
			retVal.Add(new Vector2I(x+1, y-1, 1.4142f));
		}

		return retVal.ToArray();
	}
}
