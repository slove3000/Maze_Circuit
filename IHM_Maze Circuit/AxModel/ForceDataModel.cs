﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{
    public class ForceDataModel : aXdataModel
    {
        #region Fields
        /// <summary>
        /// Force Reçu UART
        /// </summary>
        private int _forceX, _forceY;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ForceDataModel class.
        /// </summary>
        /// <param name="coX"></param>
        /// <param name="coY"></param>
        public ForceDataModel(int foX, int foY)
        {
            this._forceX = foX;
            this._forceY = foY;
        }

        public int ForceX
        {
            get
            {
                return _forceX;
            }
            set
            {
                _forceX = value;
            }
        }

        public int ForceY
        {
            get
            {
                return _forceY;
            }
            set
            {
                _forceY = value;
            }
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        // TODO : ajouter methode convertion en couple
        #endregion
    }
}
