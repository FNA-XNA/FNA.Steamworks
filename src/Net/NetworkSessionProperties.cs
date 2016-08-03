#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public class NetworkSessionProperties : IList<int?>, ICollection<int?>, IEnumerable<int?>, IEnumerable
	{
		#region Public Properties

		public int Count
		{
			get
			{
				return properties.Count;
			}
		}

		public int? this[int index]
		{
			get
			{
				return properties[index];
			}
			set
			{
				properties[index] = value;
			}
		}

		#endregion

		#region Private Variables

		private List<int?> properties;

		#endregion

		#region Public Constructor

		public NetworkSessionProperties()
		{
			properties = new List<int?>();
		}

		#endregion

		#region Public Methods

		public IEnumerator<int?> GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		#endregion

		#region IList Implementation

		int IList<int?>.IndexOf(int? item)
		{
			return properties.IndexOf(item);
		}

		void IList<int?>.Insert(int index, int? item)
		{
			properties.Insert(index, item);
		}

		void IList<int?>.RemoveAt(int index)
		{
			properties.RemoveAt(index);
		}

		#endregion

		#region ICollection Implementation

		bool ICollection<int?>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		void ICollection<int?>.Add(int? item)
		{
			properties.Add(item);
		}

		bool ICollection<int?>.Remove(int? item)
		{
			return properties.Remove(item);
		}

		bool ICollection<int?>.Contains(int? item)
		{
			return properties.Contains(item);
		}

		void ICollection<int?>.Clear()
		{
			properties.Clear();
		}

		void ICollection<int?>.CopyTo(int?[] array, int arrayIndex)
		{
			properties.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable Implementation

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}
