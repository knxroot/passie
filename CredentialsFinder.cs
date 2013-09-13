//---------------------------------------------------------------------
// <copyright file="CredentialsFinder.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using mshtml;
using System.Collections.Generic;

namespace PassIE
{
    public static class CredentialsFinder
    {
        private static List<IHTMLElement> Path(IHTMLElement element)
        {
            var path = new List<IHTMLElement>();
            while (element.parentElement != null)
            {
                path.Add(element.parentElement);
                element = element.parentElement;
            }

            return path;
        }

        private static int JointDistance<T>(List<T> path1, List<T> path2)
        {
            int distance = 0;

            while (path1.Count > distance && path2.Count > distance && path1[path1.Count - distance - 1].Equals(path2[path2.Count - distance - 1]))
            {
                ++distance;
            }

            return distance;
        }

        private static IEnumerable<IHTMLElement> FindPassword(HTMLDocument document)
        {
            IHTMLElement2 body = (IHTMLElement2)document.body;

            foreach (IHTMLElement htmlElement in body.getElementsByTagName("input"))
            {
                string type = htmlElement.getAttribute("type");

                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                if (type.ToUpper() == "PASSWORD")
                {

                    yield return htmlElement;
                }
            }
        }

        private static IHTMLElement FindClosestForm(IHTMLElement element)
        {
            IHTMLElement parent = element.parentElement;

            while (parent != null)
            {
                if (parent.tagName.ToUpper() == "FORM")
                {
                    return parent;
                }

                parent = parent.parentElement;
            }

            return null;
        }


        private static IEnumerable<IHTMLElement> FindUsername(IHTMLElement passwordElement)
        {
            IHTMLElement2 root = (IHTMLElement2)FindClosestForm(passwordElement);

            if (root == null)
            {
                root = passwordElement.document.Body;
            }

            if (root != null)
            {
                foreach (IHTMLElement htmlElement in root.getElementsByTagName("input"))
                {
                    if (htmlElement != passwordElement)
                    {
                        string type = htmlElement.getAttribute("type");

                        if (type.ToUpper() != "TEXT" && type.ToUpper() != "EMAIL")
                        {
                            continue;
                        }

                        yield return htmlElement;
                    }
                }
            }
        }
        
        public static Dictionary<IHTMLElement, IHTMLElement> FindCredentials(HTMLDocument document)
        {
            var credentialFields = new Dictionary<IHTMLElement, IHTMLElement>();
            foreach (IHTMLElement passwordElement in FindPassword(document))
            {
                List<IHTMLElement> pathToPassword = Path(passwordElement);

                IHTMLElement usernameField = null;
                int maxJointDistance = 0;

                foreach (IHTMLElement possibleUserName in FindUsername(passwordElement))
                {
                    List<IHTMLElement> pathToUsername = Path(possibleUserName);
                    int distance = JointDistance(pathToPassword, pathToUsername);

                    if (usernameField == null || distance > maxJointDistance)
                    {
                        usernameField = possibleUserName;
                        maxJointDistance = distance;
                    }
                }

                credentialFields[passwordElement] = usernameField;
            }

            return credentialFields;
        }
    }
}
