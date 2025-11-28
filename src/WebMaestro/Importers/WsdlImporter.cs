using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebMaestro.Models;

namespace WebMaestro.Importers
{
    internal class WsdlImporter : Importer
    {
        private enum SoapVersions
        {
            SOAP11 = 0,
            SOAP12 = 1
        }

        private const string NS_SOAP_11 = "http://schemas.xmlsoap.org/wsdl/soap/";
        private const string NS_SOAP_12 = "http://schemas.xmlsoap.org/wsdl/soap12/";
        private const string NS_SOAP_ENV = "http://schemas.xmlsoap.org/soap/envelope/";
        
        private const string SOAP_ENV = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Header></SOAP-ENV:Header><SOAP-ENV:Body></SOAP-ENV:Body></SOAP-ENV:Envelope>";
        private string baseUrl;

        public WsdlImporter()
        {

        }

        public WsdlImporter(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public override void Import(Stream stream)
        {
            var doc = new XmlDocument();

            doc.Load(stream);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            nsmgr.AddNamespace("soap11", NS_SOAP_11);
            nsmgr.AddNamespace("soap12", NS_SOAP_12);
            nsmgr.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");

            var definitionsNode = doc.SelectSingleNode("wsdl:definitions", nsmgr);

            var targetNamespace = definitionsNode.Attributes["targetNamespace"]?.Value;

            var tnsPrefix = definitionsNode.GetPrefixOfNamespace(targetNamespace);

            var serviceNode = definitionsNode.SelectSingleNode("wsdl:service", nsmgr);

            base.Collection.Name = serviceNode.Attributes["name"].Value;

            var portNodes = serviceNode.SelectNodes("wsdl:port", nsmgr);

            foreach (XmlElement portNode in portNodes)
            {
                var name = portNode.GetAttribute("name");
                var bindingName = portNode.GetAttribute("binding")[(tnsPrefix.Length + 1)..];

                var addressNode = portNode.FirstChild;

                var uri = addressNode.Attributes["location"].Value;

                var environment = new EnvironmentModel
                {
                    Name = name
                };

                environment.Variables.Add(new VariableModel("baseUrl", uri, "Base URL for the service"));

                Collection.Environments.Add(environment);

                var bindingNode = definitionsNode.SelectSingleNode($"wsdl:binding[@name='{ bindingName }']", nsmgr);
                var bindingTypeName = bindingNode.Attributes["type"].Value[(tnsPrefix.Length + 1)..];
                var portTypeNode = definitionsNode.SelectSingleNode($"wsdl:portType[@name='{ bindingTypeName }']", nsmgr);

                foreach (XmlElement operationNode in bindingNode.SelectNodes("wsdl:operation", nsmgr))
                {
                    var operationName = operationNode.GetAttribute("name");

                    var soapAction = operationNode.FirstChild.Attributes["soapAction"].Value;

                    var soapVersion = GetSoapVersion(operationNode.FirstChild.NamespaceURI);

                    var contentType = soapVersion switch
                    {
                        SoapVersions.SOAP12 => "application/soap+xml",
                        SoapVersions.SOAP11 => "text/xml",
                        _ => ""
                    };

                    var actionHeaderName = soapVersion switch
                    {
                        SoapVersions.SOAP12 => "Action",
                        SoapVersions.SOAP11 => "SOAPAction",
                        _ => ""
                    };

                    var inputMsgName = portTypeNode.SelectSingleNode($"wsdl:operation[@name='{ operationName }']/wsdl:input/@message", nsmgr)
                                        .Value[(tnsPrefix.Length + 1)..];

                    var messageNode = definitionsNode.SelectSingleNode($"wsdl:message[@name='{ inputMsgName }']", nsmgr);
                    var isBody = operationNode.SelectSingleNode("wsdl:input", nsmgr).FirstChild.LocalName == "body";

                    if (isBody)
                    {
                        var parts = operationNode.SelectSingleNode("wsdl:input/*[local-name()='body']", nsmgr).Attributes["parts"]?.Value;
                        XmlNode partNode;

                        // I assume here that if it is for body that only ONE part exists in the message.
                        if (parts is not null)
                        {
                            partNode = messageNode.SelectSingleNode($"wsdl:part[@name='{ parts }']", nsmgr);
                        }
                        else
                        {
                            partNode = messageNode.FirstChild;
                        }

                        var elementName = partNode.Attributes["element"]?.Value[(tnsPrefix.Length + 1)..];
                        var typeName = partNode.Attributes["type"]?.Value[(tnsPrefix.Length + 1)..];

                        if (elementName is not null)
                        {

                        }
                        else if (typeName is not null)
                        {

                        }
                        
                    }

                    string envelopeXml = CreateEnvelope();

                    Requests.Add(new()
                    {
                        Name = $"{ bindingName }_{ operationName }",
                        HttpMethod = ViewModels.HttpMethods.POST,
                        Headers = {
                            new (actionHeaderName, soapAction),
                            new ("Content-Type", contentType)
                        },
                        Body = envelopeXml
                    });
                }
            }
        }

        private static string CreateEnvelope()
        {
            var envelope = new XmlDocument();
            envelope.LoadXml(SOAP_ENV);

            string envelopeXml;
            using (var sw = new StringWriter())
            using (var xw = new XmlTextWriter(sw) { Formatting = System.Xml.Formatting.Indented })
            {
                envelope.WriteContentTo(xw);
                xw.Flush();
                envelopeXml = sw.ToString();
            }

            return envelopeXml;
        }

        private static SoapVersions GetSoapVersion(string namespaceUri)
        {
            return namespaceUri.ToLowerInvariant() switch
            {
                NS_SOAP_12 => SoapVersions.SOAP12,
                NS_SOAP_11 => SoapVersions.SOAP11,
                _ => SoapVersions.SOAP11,
            };
        }
    }
}
