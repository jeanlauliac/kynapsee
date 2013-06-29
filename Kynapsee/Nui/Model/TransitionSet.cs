using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Office.Interop.PowerPoint;

namespace Kynapsee.Nui.Model
{
    public class TransitionSet
    {
        static readonly XmlSerializer Xml = new XmlSerializer(typeof(TransitionSet));

        public List<Transition> Transitions { get; set; }

        /// <summary>
        /// Convert the model to an XML string representation.
        /// </summary>
        /// <returns>XML representation.</returns>
        public override string ToString()
        {
            var wr = new StringWriter();
            Xml.Serialize(wr, this);
            return wr.ToString();
        }

        /// <summary>
        /// Creates the model from a XML string.
        /// </summary>
        /// <param name="data">XML data.</param>
        /// <returns>Model.</returns>
        public static TransitionSet FromString(string data, GestureSet gestures, Presentation pres)
        {
            var tr = (TransitionSet)Xml.Deserialize(new StringReader(data));
            tr.Transitions.ForEach((x) => x.Bind(gestures, pres));
            return tr;
        }
    }
}
