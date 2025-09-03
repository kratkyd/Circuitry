using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuitry
{
    public static class Data
    {
		public static Size clientSize = new Size(1280, 720);
		public static List<(string name, Type gateType)> baseOptions = new List<(string name, Type gateType)>
		{
			("1", typeof(OneGate)),
			("0", typeof(ZeroGate)),
			("NOT", typeof(NotGate)),
			("AND", typeof(AndGate)),
			("NAND", typeof(NandGate)),
			("OR", typeof(OrGate)),
			("NOR", typeof(NorGate)),
			("XOR", typeof(XorGate)),
			("Blank", typeof(BlankGate)),
			("Input", typeof(InputGate)),
			("Output", typeof(OutputGate))
		};
	}
}
