﻿#region License

//  	Copyright 2013-2014 Matthew Ducker
//  	
//  	Licensed under the Apache License, Version 2.0 (the "License");
//  	you may not use this file except in compliance with the License.
//  	
//  	You may obtain a copy of the License at
//  		
//  		http://www.apache.org/licenses/LICENSE-2.0
//  	
//  	Unless required by applicable law or agreed to in writing, software
//  	distributed under the License is distributed on an "AS IS" BASIS,
//  	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  	See the License for the specific language governing permissions and 
//  	limitations under the License.

#endregion

using System;
using ObscurCore.Cryptography.Support;

namespace ObscurCore.Cryptography.Information.EllipticCurve
{
    public class DjbEcInformation : EcCurveInformation
    {
        /// <summary>
        ///     Get parameter object for performing computations.
        /// </summary>
        /// <returns></returns>
        public override ECDomainParameters GetParameters()
        {
            throw new NotSupportedException("These curves are implemented in a non-compatible way.");
        }
    }
}
