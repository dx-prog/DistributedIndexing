﻿/***********************************************************************************
 * Copyright 2017  David Garcia
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * *********************************************************************************/

namespace Sprockets.DocumentIndexer.Lucene.Types {
    public class BinaryOperatorSanitizerToken : OperatorSanitizerToken {
        public BinaryOperatorSanitizerToken(string value) : base(value) {
        }

        public CodeSanitizerToken Left { get; set; }
        public CodeSanitizerToken Right { get; set; }

        public override string Value {
            get {
                Left?.IncreasePadRight();
                Right?.IncreasePadLeft();
                return Left + OriginalValue + Right;
            }
        }

        public override int Priority => Value == "AND" ? 90 : 80;


        public override void Sanitize() {
            base.Sanitize();
            Left?.Sanitize();
            Right?.Sanitize();
        }


        public override bool IsOperand
        {
            get
            {
                if (Left == null)
                    return false;
                if (Right == null)
                    return false;
                return Left.IsOperand && Right.IsOperand;
            }
        }
    }
}