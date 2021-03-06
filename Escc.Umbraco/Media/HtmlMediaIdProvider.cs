﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Media
{
    /// <summary>
    /// Gets the ids of media items linked within a property that contains HTML
    /// </summary>
    public class HtmlMediaIdProvider : IMediaIdProvider
    {
        private readonly List<string> _propertyEditorAlises = new List<string>();
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlMediaIdProvider" /> class.
        /// </summary>
        /// <param name="propertyEditorAliases">The aliases of the property editors that might be responsible for saving media ids.</param>
        /// <param name="mediaService">The Umbraco media service</param>
        /// <exception cref="ArgumentNullException">mediaService</exception>
        public HtmlMediaIdProvider(IEnumerable<string> propertyEditorAliases, IMediaService mediaService)
        {
            if (propertyEditorAliases == null)
            {
                throw new ArgumentNullException(nameof(propertyEditorAliases));
            }

            _propertyEditorAlises.AddRange(propertyEditorAliases);
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(PropertyType propertyType)
        {
            return _propertyEditorAlises.Contains(propertyType.PropertyEditorAlias, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public IEnumerable<int> ReadProperty(Property property)
        {
            var mediaIds = new List<int>();

            if (property != null && property.Value != null && !String.IsNullOrEmpty(property.Value.ToString()))
            {
                var html = new HtmlDocument();
                html.LoadHtml(property.Value.ToString());
                var mediaLinks = html.DocumentNode.SelectNodes("//a[contains(@href,'/media/')]");
                if (mediaLinks != null)
                {
                    foreach (var mediaLink in mediaLinks)
                    {
                        var uri = new Uri(mediaLink.Attributes["href"].Value, UriKind.RelativeOrAbsolute);

                        var mediaItem = _mediaService.GetMediaByPath(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());
                        if (mediaItem != null)
                        {
                            mediaIds.Add(mediaItem.Id);
                        }
                    }
                }

            }

            return mediaIds;
        }
    }
}
