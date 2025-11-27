using System;
using System.Text.RegularExpressions;

namespace Unity.Services.Vivox
{
    /// <summary>
    /// The unique identifier for a channel. Channels are created and destroyed automatically on demand.
    /// </summary>
    internal class ChannelId
    {
        internal string GetUriDesignator(ChannelType value)
        {
            switch (value)
            {
                case ChannelType.Echo:
                    return "e";
                case ChannelType.NonPositional:
                    return "g";
                case ChannelType.Positional:
                    return "d";
            }
            throw new ArgumentException($"{GetType().Name}: {value} has no GetUriDesignator() support");
        }

        private readonly string _domain;

        public ChannelId(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return;

            var matchGroups = new Regex("sip:confctl-(?<uriDesignator>e|g|d)-(?<issuer>[^.]+).(?<channelName>[^!@.]+)(?:.(?<envId>[a-zA-Z0-9-]+))?(?:!p-(?<positionalProps>[^@]+))?@(?<domain>[a-zA-Z0-9.]+)").Match(uri);

            if (matchGroups == null || !matchGroups.Success || matchGroups.Groups.Count < 4)
            {
                throw new ArgumentException($"'{uri}' is not a valid URI");
            }

            var type = matchGroups.Groups["uriDesignator"].Value;
            switch (type)
            {
                case "g":
                    Type = ChannelType.NonPositional;
                    break;
                case "e":
                    Type = ChannelType.Echo;
                    break;
                case "d":
                    Type = ChannelType.Positional;
                    break;
                default:
                    throw new ArgumentException($"{GetType().Name}: {uri} is not a valid URI");
            }
            Issuer = matchGroups.Groups["issuer"].Value;
            Name = matchGroups.Groups["channelName"].Value;
            EnvironmentId = matchGroups.Groups["envId"].Value;
            if (Type == ChannelType.Positional)
            {
                var props = matchGroups.Groups["positionalProps"].Value;
                Properties = new Channel3DProperties(props);
            }
            _domain = string.IsNullOrEmpty(Client.defaultRealm) ? matchGroups.Groups["domain"].Value : Client.defaultRealm;
        }

        /// <summary>
        /// A constructor for creating an echo or non-positional channel.
        /// </summary>
        /// <param name="issuer">The issuer that is responsible for authorizing access to this channel.</param>
        /// <param name="name">The name of this channel.</param>
        /// <param name="domain">The Vivox domain that hosts this channel.</param>
        /// <param name="type">The channel type.</param>
        /// <param name="properties">The 3D positional channel properties.</param>
        /// <param name="environmentId">Environment ID for Unity Game Service-specific integrations </param>
        public ChannelId(string issuer, string name, string domain, ChannelType type = ChannelType.NonPositional, Channel3DProperties properties = null, string environmentId = null)
        {
            if (string.IsNullOrEmpty(issuer)) throw new ArgumentNullException(nameof(issuer));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));
            // EnvironmentId is not required but if we have it we treat it as a seperate section in the URI and need to surround it with dots.
            if (!string.IsNullOrEmpty(environmentId)) EnvironmentId = environmentId;
            if (!Enum.IsDefined(typeof(ChannelType), type)) throw new ArgumentOutOfRangeException(type.ToString());
            if (properties == null) Properties = new Channel3DProperties();
            else if (type == ChannelType.Positional && !properties.IsValid()) throw new ArgumentException("", nameof(properties));
            else Properties = properties;
            if (!IsValidName(name)) throw new ArgumentException($"{GetType().Name}: Argument contains one, or more, invalid characters, or the length of the name exceeds 200 characters.", nameof(name));

            Issuer = issuer;
            Name = name;
            _domain = string.IsNullOrEmpty(Client.defaultRealm) ? domain : Client.defaultRealm;
            Type = type;
        }

        /// <summary>
        /// The issuer that is responsible for authorizing access to this channel.
        /// </summary>
        public string Issuer { get; }
        /// <summary>
        /// The name of this channel.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The UAS Environment ID of this channel.
        /// </summary>
        public string EnvironmentId { get; }
        /// <summary>
        /// This is a value that your developer support representative provides. It is subject to change if a different server-determined destination is provided during client connector creation.
        /// </summary>
        public string Domain => string.IsNullOrEmpty(Client.defaultRealm) ? _domain : Client.defaultRealm;
        /// <summary>
        /// The channel type.
        /// </summary>
        public ChannelType Type { get; }
        /// <summary>
        /// The 3D channel properties.
        /// </summary>
        public Channel3DProperties Properties { get; }

        /// <summary>
        /// Determine if two objects are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the objects are of equal value.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType() != obj.GetType()) return false;

            return Equals((ChannelId)obj);
        }

        bool Equals(ChannelId other)
        {
            return string.Equals(_domain, other._domain) && string.Equals(Name, other.Name) &&
                string.Equals(Issuer, other.Issuer) && Type == other.Type;
        }

        /// <summary>
        /// A hash function for ChannelId.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hc = _domain?.GetHashCode() ?? 0;
                hc = (hc * 397) ^ (Name?.GetHashCode() ?? 0);
                hc = (hc * 397) ^ (Issuer?.GetHashCode() ?? 0);
                hc = (hc * 397) ^ (_domain?.GetHashCode() ?? 0);
                hc = (hc * 397) ^ Type.GetHashCode();
                return hc;
            }
        }

        /// <summary>
        /// This is true if the Name, Domain, and Issuer are empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(Issuer);
        /// <summary>
        /// A test for an empty ChannelId.
        /// </summary>
        /// <param name="id">The channel ID.</param>
        /// <returns>True if the ID is null or empty.</returns>
        public static bool IsNullOrEmpty(ChannelId id)
        {
            return id == null || id.IsEmpty;
        }

        /// <summary>
        /// The network representation of the channel ID.
        /// Note: This will be refactored in the future so the internal network representation of the ChannelId is hidden.
        /// </summary>
        /// <returns>A URI for this channel.</returns>
        public override string ToString()
        {
            if (!IsValid()) return "";

            string props = Type == ChannelType.Positional ? Properties.ToString() : string.Empty;
            var uri = string.IsNullOrEmpty(EnvironmentId)
                ? $"sip:confctl-{GetUriDesignator(Type)}-{Issuer}.{Name}{props}@{Domain}"
                : $"sip:confctl-{GetUriDesignator(Type)}-{Issuer}.{Name}.{EnvironmentId}{props}@{Domain}";
            return uri;
        }

        /// <summary>
        /// Ensure that _name, _domain, and _issuer are not empty, and further validate the _name field by checking it against an array of valid characters.
        /// Note: If the channel is positional, then _properties is also validated.
        /// </summary>
        /// <returns>If the ChannelID is valid.</returns>
        internal bool IsValid()
        {
            if (IsEmpty || !IsValidName(Name) || (Type == ChannelType.Positional && !Properties.IsValid()))
                return false;
            return true;
        }

        /// <summary>
        /// Check the name value against a max length and group of valid characters.
        /// </summary>
        /// <returns>If the name is valid.</returns>
        internal bool IsValidName(string name)
        {
            if (name.Length > 200)
                return false;

            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890=+-_.!~()%";
            foreach (char c in name.ToCharArray())
            {
                if (!validChars.Contains(c.ToString()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
