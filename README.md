# BirdNest.QuickFont

*[AZDO]: Approaching Zero Driver Offset
*[VBOs]: Vertex Buffer Objects
*[SSBO]: Shader Storage Buffer Objects

Attempt to use OpenTK font rendering with AZDO techniques
 - Using forks of QuickFonts as a basis
    - Vertex buffer objects example (https://github.com/swax/QuickFont)
    - Original QuickFont 1.2 of http://www.opentk.com/project/QuickFont 
 - Code largely based on [apitest](https://github.com/nvMcJohn/apitest).

# Features
 - Bindless texture handles (ARB_bindless_textures) 
 - VBOs 
 - GLSL shaders
 - Multi indirect draw  
 - dynamic data updates with fences
 - upfront SSBO buffer initialisation to minimise OpenGL calls during update
 - (FUTURE) signed distance fonts

## Version 0.1
- Added working example of Quickfont library using .NET 4.5 
    - Library (QuickFont.proj)
    - OpenTK application (Example.proj)
- Initial commit of ADZO version
    - Library (NextFont.proj)
    - OpenTK application (NextFont.ConsoleApplication.proj)
    - C#/OpenTK translation of [apitest](https://github.com/nvMcJohn/apitest) (DynamicStreaming.proj)

# MIT LICENSE
Copyright (c) 2015 David Young

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.