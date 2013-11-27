//
//  Copyright 2013  Matthew Ducker
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace ObscurCore
{
    public sealed class KdfDescription
    {
        /// <summary>
        /// Name of the KDF scheme (must be a member of KDFDirectory).
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Name to show a user or for a detailed specification.
        /// </summary>
        public string DisplayName { get; internal set; }
    }
}