using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jamong.Server
{
    public class JamongView : MonoBehaviour
    {
        private int viewID = -1;
        private bool isMine;

        #region 프로퍼티
        public int ViewID { get => ViewID; }
        public bool IsMine { get => IsMine; }
        public bool IsInitComplete { get => ViewID > -1; }
        #endregion

        public void Init(int newID, bool isMine)
        {
            viewID = newID;
            this.isMine = isMine;
        }
    }
}
