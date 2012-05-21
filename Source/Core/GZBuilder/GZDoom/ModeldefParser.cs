﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CodeImp.DoomBuilder.ZDoom;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.GZBuilder.GZDoom;

namespace CodeImp.DoomBuilder.GZBuilder.GZDoom {
   
    public class ModeldefParser : ZDTextParser {
        public static string INVALID_TEXTURE = "**INVALID_TEXTURE**";
        
        private Dictionary<string, ModeldefEntry> modelDefEntries; //classname, entry
        public Dictionary<string, ModeldefEntry> ModelDefEntries { get { return modelDefEntries; } }

        public string Source { get { return sourcename; } }

        //should be called after all decorate actors are parsed 
        public override bool Parse(Stream stream, string sourcefilename) {
            base.Parse(stream, sourcefilename);
            modelDefEntries = new Dictionary<string, ModeldefEntry>();

            // Continue until at the end of the stream
            while (SkipWhitespace(true)) {
                string token = ReadToken();
                if (token != null) {
                    token = token.ToLowerInvariant();

                    if (token == "model") { //model structure start
                        //find classname
                        SkipWhitespace(true);
                        string className = StripTokenQuotes(ReadToken()).ToLowerInvariant();

                        if (!string.IsNullOrEmpty(className)) {
                            if (modelDefEntries.ContainsKey(className))
                                continue; //already got this class; continue to next one

                            //now find opening brace
                            SkipWhitespace(true);
                            token = ReadToken();
                            if (token != "{") {
                                GZBuilder.GZGeneral.LogAndTraceWarning("Unexpected token found in "+sourcefilename+" at line "+GetCurrentLineNumber()+": expected '{', but got " + token);
                                continue; //something wrong with modeldef declaration, continue to next one
                            }

                            ModeldefStructure mds = new ModeldefStructure();
                            ModeldefEntry mde = mds.Parse(this);
                            if (mde != null) {
                                GZBuilder.GZGeneral.Trace("Got mds for class " + className);
                                mde.ClassName = className;
                                modelDefEntries.Add(className, mde); 
                            }
                        } else {
                            continue; //no class name found. continue to next structure
                        }

                    } else {
                        // Unknown structure!
                        string token2;
                        do {
                            if (!SkipWhitespace(true)) break;
                            token2 = ReadToken();
                            if (token2 == null) break;
                        }
                        while (token2 != "{");
                        int scopelevel = 1;
                        do {
                            if (!SkipWhitespace(true)) break;
                            token2 = ReadToken();
                            if (token2 == null) break;
                            if (token2 == "{") scopelevel++;
                            if (token2 == "}") scopelevel--;
                        }
                        while (scopelevel > 0);
                    }

                }

            }

            if (modelDefEntries.Count > 0)
                return true;
            return false;
        }
    }
}
