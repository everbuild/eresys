using System;
using System.Collections;

namespace Eresys
{
	public class Collision
	{
		public float fraction;
		public Vector response;

		public Collision(float fraction, Vector response)
		{
			this.fraction = fraction;
			this.response = response;
		}
	}
}
