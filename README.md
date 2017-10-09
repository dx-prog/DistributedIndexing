# To Database?

This is a research project consisting of various tools designed
to allow users to load, edit, explore, and delete data of various types
either through web services, self-hosted applications, GUI 
tools, or command line from various sources.


---
# TOOL SET

## Sprockets Large Graph Serialization
Status: Beta
Converts CLR object large graphs into instructions that can be saved and loaded.
The class avoids the use of recursion. Useful for storing graphs and networks
which store complex related data. 

This library supports 3 forms of serialization for:
* Serializable Classes (ISerializable)
* Structures, Primatives, and Arrays thereof
* IEnumerables with some limitations



---
## License

   Copyright 2017 David Garcia

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.


