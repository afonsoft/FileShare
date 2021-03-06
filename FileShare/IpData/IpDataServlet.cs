﻿using FileShare.Models;
using IpData;
using IpData.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FileShare.IpData
{
    public class IpDataServlet
    {
        private readonly IpDataClient client;
        private readonly string ip;
        private const string key = "d39b8f6602b4476864ffc57e8c05fa5cd386818545c803a014f1fb44";
        private readonly ILogger<IpDataServlet> _logger;
        private IpInfo ipInfo;

        public IpDataServlet(string ip) : this()
        {
            this.ip = ip;
        }
        public IpDataServlet()
        {
            _logger = new Afonsoft.Logger.Logger<IpDataServlet>();
            client = new IpDataClient(key);
            this.ip = "";
        }

        public async Task<IpInfo> GetIp()
        {
            if (!string.IsNullOrEmpty(ip))
                return await client.Lookup(ip);
            return await client.Lookup();
        }

        public async Task<IpDataModel> GetIpInfo()
        {
            try
            {
                if (!string.IsNullOrEmpty(ip))
                    ipInfo = await client.Lookup(ip);
                else
                    ipInfo = await client.Lookup();

                return new IpDataModel
                {
                    Asn = ipInfo.Asn.Asn,
                    AsnDomain = ipInfo.Asn.Domain,
                    AsnName = ipInfo.Asn.Name,
                    AsnRoute = ipInfo.Asn.Route,
                    AsnType = ipInfo.Asn.Type,
                    CallingCode = ipInfo.CallingCode,
                    City = ipInfo.City,
                    ContinentCode = ipInfo.ContinentCode,
                    ContinentName = ipInfo.ContinentName,
                    CountryCode = ipInfo.CountryCode,
                    CountryName = ipInfo.CountryName,
                    Ip = ipInfo.Ip,
                    Latitude = ipInfo.Latitude,
                    Longitude = ipInfo.Longitude,
                    Organisation = ipInfo.Organisation,
                    Postal = ipInfo.Postal,
                    Region = ipInfo.Region,
                    RegionCode = ipInfo.RegionCode,
                    TimeZone = ipInfo.TimeZone.Name,
                    Languages = ipInfo.Languages[0].Name,
                    Error = ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao IpDataClient : {ex.Message}");
                return new IpDataModel { Ip = ip, Error = ex.Message };
            }
        }
    }
}