using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher
{
    class MSILNode
    {
        private DirectiveEnum type;
        private List<string> comments;
        private List<string> attributes;
        private List<MSILNode> subNodes;
        private List<string> data;

        public MSILNode(DirectiveEnum type)
        {
            this.type = type;
            comments = new List<string>();
            attributes = new List<string>(1);
            subNodes = new List<MSILNode>();
            data = new List<string>();
        }

        public MSILNode()
        {
            comments = new List<string>();
           
            subNodes = new List<MSILNode>();
        }

        public void parseString(string toParse)
        {
            string[] lines = toParse.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }
            for(int i = 0; i < lines.Length; i++)
            {
                handleString(ref lines, ref i);
                Console.WriteLine(i);
            }
        }

        public void handleString(ref string[] lines, ref int position)
        {
            if (lines[position].IndexOf("//") == 0)
            {
                comments.Add(lines[position]);
                return;
            }

            if (lines[position].IndexOf(".") == 0)
            {
                string[] declarationParts = lines[position].Split(' ');
                string declarationString = declarationParts[0].Substring(1, declarationParts[0].Length - 1);
                declarationString = char.ToUpper(declarationString[0]) + declarationString.Substring(1);
                DirectiveEnum type = (DirectiveEnum)Enum.Parse(typeof(DirectiveEnum), declarationString);
                string[] attributes = declarationParts.Skip(1).ToArray();
                MSILNode newNode = new MSILNode(type);
                newNode.addAttributes(attributes);

                while (lines[position + 1][0] != '{' && lines[position + 1][0] != '}' && lines[position + 1][0] != '.' && lines[position + 1].IndexOf("//") != 0)
                {
                    attributes = lines[++position].Split(' ');
                    newNode.addAttributes(attributes);
                }

                if (lines[position + 1][0] == '{')
                {
                    position += 2;

                    while(lines[position][0] != '}')
                    {
                        newNode.handleString(ref lines, ref position);
                        position++;
                    }

                    while (lines[position + 1].IndexOf("catch") == 0)
                    {
                        position += 3;

                        while (lines[position][0] != '}')
                        {
                            newNode.handleString(ref lines, ref position);
                            position++;
                        }
                    }

                    if (lines[position + 1] == "finally")
                    {
                        position += 3;

                        while (lines[position][0] != '}')
                        {
                            newNode.handleString(ref lines, ref position);
                            position++;
                        }
                    }
                }

                subNodes.Add(newNode);
                return;
            }

            this.data.Add(lines[position]);

        }

        public void addAttribute(string attribute)
        {
            this.attributes.Add(attribute);
        }

        public void addAttributes(string[] attributes)
        {
            this.attributes.AddRange(attributes);
        }
    }
}
