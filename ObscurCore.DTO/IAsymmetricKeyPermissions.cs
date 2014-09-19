#region License

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

namespace ObscurCore.DTO
{
    /// <summary>
    ///     Interface for data describing how and where a cryptographic key can be used.
    /// </summary>
    public interface IAsymmetricKeyPermissions
    {
        /// <summary>
        ///     Types of use for which the key is allowed (operations).
        /// </summary>
        AsymmetricKeyUsePermission UsePermissions { get; set; }

        /// <summary>
        ///     Use contexts for which the key is allowed (environment).
        /// </summary>
        KeyUseContextPermission ContextPermissions { get; set; }
    }
}
