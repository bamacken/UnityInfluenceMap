/*
Copyright (C) 2012 Anomalous Underdog

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;

public interface IPropagator
{
	Vector2I GridPosition { get; }
	float Value { get; }
}

public class SimplePropagator : MonoBehaviour, IPropagator
{
	[SerializeField]
	float _value;
	public float Value { get{ return _value; } }

	[SerializeField]
	InfluenceMapControl _map;

	CharacterController _move;

	Vector3 _bottomLeft;
	Vector3 _topRight;

	Vector3 _destination;

	Vector3 _velocity;

	[SerializeField]
	float _speed;

	public Vector2I GridPosition
	{
		get
		{
			return _map.GetGridPosition(transform.position);
		}
	}

	// Use this for initialization
	void Start()
	{
		_move = GetComponent<CharacterController>();
		_map.RegisterPropagator(this);
		_map.GetMovementLimits(out _bottomLeft, out _topRight);

		InvokeRepeating("ChooseNewDestination", 0.001f, 3.0f);
	}

	// Update is called once per frame
	void Update()
	{
		_velocity = _destination - transform.position;
		_velocity.Normalize();
		_velocity *= _speed;

		_move.SimpleMove(_velocity);
	}

	void ChooseNewDestination()
	{
		_destination = PickDestination();
	}

	Vector3 PickDestination()
	{
		return new Vector3(
			Random.Range(_bottomLeft.x, _topRight.x),
			Random.Range(_bottomLeft.y, _topRight.y),
			Random.Range(_bottomLeft.z, _topRight.z)
		);
	}
}
