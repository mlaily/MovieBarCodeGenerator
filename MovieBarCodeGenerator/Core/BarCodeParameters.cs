//Copyright 2011-2018 Melvyn Laily
//https://zerowidthjoiner.net

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class BarCodeParameters
    {
        public int Width { get; set; } = 1000;
        public int? Height { get; set; } = null;
        public int BarWidth { get; set; } = 1;
    }

    public class CompleteBarCodeGenerationParameters
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string SmoothedOutputPath { get; set; }
        public bool GenerateSmoothedOutput { get; set; }
        public BarCodeParameters BarCode { get; set; }
    }
}
