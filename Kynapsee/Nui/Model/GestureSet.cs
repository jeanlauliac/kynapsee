using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Kynapsee.Nui.Model
{
    /// <summary>
    /// Describes a full set of gesture to recognize.
    /// </summary>
    public class GestureSet
    {
        static readonly XmlSerializer Xml = new XmlSerializer(typeof(GestureSet));

        /// <summary>
        /// Gets the gestures that the model allow.
        /// </summary>
        public List<Gesture> Gestures { get; set; }

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
        public static GestureSet FromString(string data)
        {
            return (GestureSet)Xml.Deserialize(new StringReader(data));
        }
    }
}
