// ***********************************************************************
// Assembly         : WorkflowHost
// Author           : rahulrai
// Created          : 01-04-2016
//
// Last Modified By : rahulrai
// Last Modified On : 01-07-2016
// ***********************************************************************
// <copyright file="TypeSwitch.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace WorkflowHost.Utilities
{
    #region

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Class TypeSwitch.
    /// </summary>
    public class TypeSwitch
    {
        #region Fields

        /// <summary>
        /// The matches.
        /// </summary>
        private readonly Dictionary<Type, Action> matches = new Dictionary<Type, Action>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The case.
        /// </summary>
        /// <typeparam name="T">Type of input</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="TypeSwitch" />.</returns>
        public TypeSwitch Case<T>(Action action)
        {
            this.matches.Add(typeof(T), action);
            return this;
        }

        /// <summary>
        /// The switch.
        /// </summary>
        /// <param name="value">The target.</param>
        public void Switch(Type value)
        {
            this.matches[value].Invoke();
        }

        #endregion
    }
}