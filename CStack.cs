using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Inlamning_3_ra_kod
{
    /* CLASS: CStack
     * PURPOSE: Is essentially a RPN-calculator with four registers X, Y, Z, T
     *   like the HP RPN calculators. Numeric values are entered in the entry
     *   string by adding digits and one comma. For test purposes the method
     *   RollSetX can be used instead. Operations can be performed on the
     *   calculator preferrably by using one of the methods
     *     1. BinOp - merges X and Y into X via an operation and rolls down
     *        the stack
     *     2. Unop - operates X and puts the result in X with overwrite
     *     3. Nilop - adds a known constant on the stack and rolls up the stack
     */
    public class CStack
    {
        public double X, Y, Z, T;
        public string entry;
        public string varName;
        string[] vars = new string[8];
        string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\data\molkfreecalc.clc");
        public List<string> fileValues;

        /* CONSTRUCTOR: CStack
         * PURPOSE: create a new stack and init X, Y, Z, T and the text entry.
         *          If file exists, also init vars and load all values from file.
         * PARAMETERS: --
         */
        public CStack()
        {
            entry = "";

            if (File.Exists(file))
            {
                fileValues = ReadFile(file);

                // Set stack values from file
                X = double.Parse(fileValues[0]);
                Y = double.Parse(fileValues[1]);
                Z = double.Parse(fileValues[2]);
                T = double.Parse(fileValues[3]);

                // Set variables values from file
                int j = 4;
                for (int i = 0; i < vars.Length; i++)
                {
                    vars[i] = fileValues[j];
                    j++;
                }
            }
            else
            {
                X = Y = Z = T = 0;
            }
        }

        /* METHOD: ReadFile
         * PURPOSE: Read textfile and set values in a list
         * PARAMETERS: file - textfile with delimiter ';'
         * RETURNS: List containing all values from the file
         */
        public List<string> ReadFile(string file)
        {
            List<string> fileValues;
            using (StreamReader sr = new StreamReader(file))
            {
                fileValues = sr.ReadToEnd().ToString().Split(';').ToList();
            }
            return fileValues;
        }

        /* METHOD: Exit
         * PURPOSE: called on exit, saves stack and variables to file with delimiter ';'
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Exit()
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < vars.Length; i++)
                {
                    sb.Append($"{vars[i]};");
                }

                sw.Write(@"{0};{1};{2};{3};{4}", X, Y, Z, T, sb.ToString());
            }
        }
        /* METHOD: StackString
         * PURPOSE: construct a string to write out in a stack view
         * PARAMETERS: --
         * RETURNS: the string containing the values T, Z, Y, X with newlines 
         *   between them
         */
        public string StackString()
        {
            return $"{T}\n{Z}\n{Y}\n{X}\n{entry}";
        }
        /* METHOD: VarString
         * PURPOSE: construct a string to write out in a variable list
         * PARAMETERS: --
         * RETURNS: the string containing the values in array "vars" with newlines between them
         */
        public string VarString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vars.Length; i++)
            {
                sb.Append($"{vars[i]}\n");
            }
            sb.Append(varName);
            return sb.ToString();
        }
        /* METHOD: SetX
         * PURPOSE: set X with overwrite
         * PARAMETERS: double newX - the new value to put in X
         * RETURNS: --
         */
        public void SetX(double newX)
        {
            X = newX;
        }
        /* METHOD: EntryAddNum
         * PURPOSE: add a digit to the entry string
         * PARAMETERS: string digit - the candidate digit to add at the end of the
         *   string
         * RETURNS: --
         * FAILS: if the string digit does not contain a parseable integer, nothing
         *   is added to the entry
         */
        public void EntryAddNum(string digit)
        {
            int val;
            if (int.TryParse(digit, out val))
            {
                entry = entry + val;
            }
        }
        /* METHOD: EntryAddComma
         * PURPOSE: adds a comma to the entry string
         * PARAMETERS: --
         * RETURNS: --
         * FAILS: if the entry string already contains a comma, nothing is added
         *   to the entry
         */
        public void EntryAddComma()
        {
            if (entry.IndexOf(",") == -1)
                entry = entry + ",";
        }
        /* METHOD: EntryChangeSign
         * PURPOSE: changes the sign of the entry string
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: if the first char is already a '-' it is exchanged for a '+',
         *   if it is a '+' it is changed to a '-', otherwise a '-' is just added
         *   first
         */
        public void EntryChangeSign()
        {
            char[] cval = entry.ToCharArray();
            if (cval.Length > 0)
            {
                switch (cval[0])
                {
                    case '+': cval[0] = '-'; entry = new string(cval); break;
                    case '-': cval[0] = '+'; entry = new string(cval); break;
                    default: entry = '-' + entry; break;
                }
            }
            else
            {
                entry = '-' + entry;
            }
        }
        /* METHOD: Enter
         * PURPOSE: converts the entry to a double and puts it into X
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: the entry is cleared after a successful operation
         */
        public void Enter()
        {
            if (entry != "")
            {
                RollSetX(double.Parse(entry));
                entry = "";
            }
        }
        /* METHOD: Drop
         * PURPOSE: drops the value of X, and rolls down
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: Z gets the value of T
         */
        public void Drop()
        {
            X = Y; Y = Z; Z = T;
        }
        /* METHOD: DropSetX
         * PURPOSE: replaces the value of X, and rolls down
         * PARAMETERS: double newX - the new value to assign to X
         * RETURNS: --
         * FEATURES: Z gets the value of T
         * NOTES: this is used when applying binary operations consuming
         *   X and Y and putting the result in X, while rolling down the
         *   stack
         */
        public void DropSetX(double newX)
        {
            X = newX; Y = Z; Z = T;
        }
        /* METHOD: BinOp
         * PURPOSE: evaluates a binary operation
         * PARAMETERS: string op - the binary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled down
         */
        public void BinOp(string op)
        {
            switch (op)
            {
                case "+": DropSetX(Y + X); break;
                case "−": DropSetX(Y - X); break;
                case "×": DropSetX(Y * X); break;
                case "÷": DropSetX(Y / X); break;
                case "yˣ": DropSetX(Math.Pow(Y, X)); break;
                case "ˣ√y": DropSetX(Math.Pow(Y, 1.0 / X)); break;
            }
        }
        /* METHOD: Unop
         * PURPOSE: evaluates a unary operation
         * PARAMETERS: string op - the unary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is not moved, X is replaced by the result of
         *   the operation
         */
        public void Unop(string op)
        {
            switch (op)
            {
                // Powers & Logarithms:
                case "x²": SetX(X * X); break;
                case "√x": SetX(Math.Sqrt(X)); break;
                case "log x": SetX(Math.Log10(X)); break;
                case "ln x": SetX(Math.Log(X)); break;
                case "10ˣ": SetX(Math.Pow(10, X)); break;
                case "eˣ": SetX(Math.Exp(X)); break;

                // Trigonometry:
                case "sin": SetX(Math.Sin(X)); break;
                case "cos": SetX(Math.Cos(X)); break;
                case "tan": SetX(Math.Tan(X)); break;
                case "sin⁻¹": SetX(Math.Asin(X)); break;
                case "cos⁻¹": SetX(Math.Acos(X)); break;
                case "tan⁻¹": SetX(Math.Atan(X)); break;
            }
        }
        /* METHOD: Nilop
         * PURPOSE: evaluates a "nilary operation" (insertion of a constant)
         * PARAMETERS: string op - the nilary operation (name of the constant)
         *   retrieved from the GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled up, X is preserved in Y that is preserved in
         *   Z that is preserved in T, T is erased
         */
        public void Nilop(string op)
        {
            switch (op)
            {
                case "π": RollSetX(Math.PI); break;
                case "e": RollSetX(Math.E); break;
            }
        }
        /* METHOD: Roll
         * PURPOSE: rolls the stack up
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Roll()
        {
            double tmp = T;
            T = Z; Z = Y; Y = X; X = tmp;
        }
        /* METHOD: Roll
         * PURPOSE: rolls the stack up and puts a new value in X
         * PARAMETERS: double newX - the new value to put into X
         * RETURNS: --
         * FEATURES: T is dropped
         */
        public void RollSetX(double newX)
        {
            T = Z; Z = Y; Y = X; X = newX;
        }
        /* METHOD: SetAddress
         * PURPOSE: Set varName to the last variable-button pressed.
         * PARAMETERS: name - name of the button pressed
         * RETURNS: --
         */
        public void SetAddress(string name)
        {
            varName = name;
        }
        /* METHOD: SetVar
         * PURPOSE: Set value to a variable in the array 'vars'
         * PARAMETERS: --
         * RETURNS: --
         */
        public void SetVar()
        {
            switch (varName)
            {
                case "A": vars[0] = X.ToString(); break;
                case "B": vars[1] = X.ToString(); break;
                case "C": vars[2] = X.ToString(); break;
                case "D": vars[3] = X.ToString(); break;
                case "E": vars[4] = X.ToString(); break;
                case "F": vars[5] = X.ToString(); break;
                case "G": vars[6] = X.ToString(); break;
                case "H": vars[7] = X.ToString(); break;
            }
        }
        /* METHOD: GetVar
         * PURPOSE: Set X to a value from a variable
         * PARAMETERS: --
         * RETURNS: --
         */
        public void GetVar()
        {
            switch (varName)
            {
                case "A": RollSetX(double.Parse(vars[0])); break;
                case "B": RollSetX(double.Parse(vars[1])); break;
                case "C": RollSetX(double.Parse(vars[2])); break;
                case "D": RollSetX(double.Parse(vars[3])); break;
                case "E": RollSetX(double.Parse(vars[4])); break;
                case "F": RollSetX(double.Parse(vars[5])); break;
                case "G": RollSetX(double.Parse(vars[6])); break;
                case "H": RollSetX(double.Parse(vars[7])); break;
            }
        }
    }
}
