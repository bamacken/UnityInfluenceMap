/*
Copyright (C) 2012 Anomalous Underdog

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;

public class InfluenceMapControl : MonoBehaviour
{
	[SerializeField]
	Transform _bottomLeft;
	
	[SerializeField]
	Transform _upperRight;
	
	[SerializeField]
	float _gridSize;
	
	[SerializeField]
	float _decay = 0.3f;
	
	[SerializeField]
	float _momentum = 0.8f;
	
	[SerializeField]
	int _updateFrequency = 3;
	
	InfluenceMap _influenceMap;

	[SerializeField]
	GridDisplay _display;

	void CreateMap()
	{
		// how many of gridsize is in Mathf.Abs(_upperRight.positon.x - _bottomLeft.position.x)
		int width = (int)(Mathf.Abs(_upperRight.position.x - _bottomLeft.position.x) / _gridSize);
		int height = (int)(Mathf.Abs(_upperRight.position.z - _bottomLeft.position.z) / _gridSize);
		
		Debug.Log(width + " x " + height);
		
		_influenceMap = new InfluenceMap(width, height, _decay, _momentum);
		
		_display.SetGridData(_influenceMap);
		_display.CreateMesh(_bottomLeft.position, _gridSize);
	}

	public void RegisterPropagator(IPropagator p)
	{
		_influenceMap.RegisterPropagator(p);
	}

	public Vector2I GetGridPosition(Vector3 pos)
	{
		int x = (int)((pos.x - _bottomLeft.position.x)/_gridSize);
		int y = (int)((pos.z - _bottomLeft.position.z)/_gridSize);

		return new Vector2I(x, y);
	}

	public void GetMovementLimits(out Vector3 bottomLeft, out Vector3 topRight)
	{
		bottomLeft = _bottomLeft.position;
		topRight = _upperRight.position;
	}
	
	void Awake()
	{
		CreateMap();
		
		InvokeRepeating("PropagationUpdate", 0.001f, 1.0f/_updateFrequency);
	}

	void PropagationUpdate()
	{
		_influenceMap.Propagate();
	}

	void SetInfluence(int x, int y, float value)
	{
		_influenceMap.SetInfluence(x, y, value);
	}

	void SetInfluence(Vector2I pos, float value)
	{
		_influenceMap.SetInfluence(pos, value);
	}

	void Update()
	{
		_influenceMap.Decay = _decay;
		_influenceMap.Momentum = _momentum;
		
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit mouseHit;
		if (Physics.Raycast(mouseRay, out mouseHit) && Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			// is it within the grid
			// if so, call SetInfluence in that grid position to 1.0
			Vector3 hit = mouseHit.point;
			if (hit.x > _bottomLeft.position.x && hit.x < _upperRight.position.x && hit.z > _bottomLeft.position.z && hit.z < _upperRight.position.z)
			{
				Vector2I gridPos = GetGridPosition(hit);

				//Debug.Log("hit " + x + " " + y);
				if (gridPos.x < _influenceMap.Width && gridPos.y < _influenceMap.Height)
				{
					SetInfluence(gridPos, (Input.GetMouseButton(0) ? 1.0f : -1.0f));
				}
			}
		}
	}
}
