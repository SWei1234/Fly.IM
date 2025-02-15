﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using Consul;
using Fly.Core.Models;

namespace Fly.Core.Extensions
{
    public static class ConsulBuilderExtensions
    {
        public static IApplicationBuilder RegisterConsul(this IApplicationBuilder app, IApplicationLifetime lifetime,
            ConsulOption consulOption)
        {
            var consulClient = new ConsulClient(x => { x.Address = new Uri(consulOption.ConsulAddress); });

            var registration = new AgentServiceRegistration
            {
                ID = Guid.NewGuid().ToString(),
                Name = consulOption.ServiceName,//服务名
                Address = consulOption.ServiceIp,//服务绑定IP
                Port = consulOption.ServicePort,//服务绑定端口
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//监控检查时间间隔
                    HTTP =  consulOption.ServiceHealthCheck,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            };
            //服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();

            //应用程序终止时，服务取消注册
            lifetime.ApplicationStopping.Register(() =>
                {
                    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                });

            return app;
        }
    }
}
