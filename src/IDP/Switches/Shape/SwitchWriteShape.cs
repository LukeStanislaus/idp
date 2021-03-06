﻿// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using IDP.Processors;
using IDP.Processors.RouterDb;
using Itinero.IO.Shape;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace IDP.Switches.Shape
{
    /// <summary>
    /// A switch to write a shapefile.
    /// </summary>
    class SwitchWriteShape : Switch
    {
        /// <summary>
        /// Creates a switch to write a shapefile.
        /// </summary>
        public SwitchWriteShape(string[] a)
            : base(a)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new string[] { "--write-shape" };
            }
        }

        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (this.Arguments.Length != 1) { throw new ArgumentException("Exactly one argument is expected."); }
            if (previous.Count < 1) { throw new ArgumentException("Expected at least one processors before this one."); }

            var file = new FileInfo(this.Arguments[0]);

            if (!(previous[previous.Count - 1] is Processors.RouterDb.IProcessorRouterDbSource))
            {
                throw new Exception("Expected a router db source.");
            }

            var source = (previous[previous.Count - 1] as Processors.RouterDb.IProcessorRouterDbSource);
            Func<Itinero.RouterDb> getRouterDb = () =>
            {
                var routerDb = source.GetRouterDb();

                var profiles = new List<Profile>();
                foreach(var vehicle in routerDb.GetSupportedVehicles())
                {
                    profiles.Add(vehicle.Fastest());
                }
                
                routerDb.WriteToShape(file.FullName, profiles.ToArray());
                return routerDb;
            };

            processor = new Processors.RouterDb.ProcessorRouterDbSource(getRouterDb);

            return 1;
        }
    }
}
