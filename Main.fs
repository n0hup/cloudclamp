﻿namespace CloudClamp

// external
open System
open System.Reflection

// internal
open Cli
open Stage

module Main =

  let callModuleFunction (moduleName:string) (functionName:string) = 
    let asm = Assembly.GetExecutingAssembly()
    for t in asm.GetTypes() do
      for m in t.GetMethods() do
        if t.FullName = moduleName && m.IsStatic && m.Name = functionName then 
          Console.WriteLine(String.Format("Invoking: {0}.{1}", t.FullName, m.Name))
          m.Invoke(null, [||]) |> ignore

  [<EntryPoint>]
  let main argv =
    
    let commandLineArgumentsParsed = parseCommandLine (Array.toList argv)
    Console.WriteLine("{0}", commandLineArgumentsParsed)
    
    let stage = fromStageToString commandLineArgumentsParsed.Stage

    callModuleFunction commandLineArgumentsParsed.Service stage

    //return
    0

  // REDIRECT ONLY


  // resource "aws_s3_bucket" "s3-bucket" {
  //   bucket = var.bucket-name
  //   acl    = var.acl
  //   region = var.region

  //   website {
  //     redirect_all_requests_to = var.redirect-all-requests-to
  //   }

  //   tags = {
  //     Name        = format("%s-s3-bucket", var.bucket-name)
  //     Environment = var.environment
  //     Stage       = var.stage
  //     Scope       = var.scope
  //   }
  // }

  // module "static-website-apex" {
  //   source                   = "./modules/static-website-redirect-only/"
  //   acl                      = "public-read"
  //   bucket-name              = "lambdainsight.com"
  //   environment              = "website"
  //   redirect-all-requests-to = "www.lambdainsight.com"
  //   region                   = "eu-central-1"
  //   scope                    = "global"
  //   stage                    = "prod"
  // }

  // WWW

  // resource "aws_s3_bucket" "s3-bucket" {
  //   bucket = var.bucket-name
  //   acl    = var.acl
  //   region = var.region

  //   website {
  //     index_document           = length(var.redirect-all-requests-to) > 0 ? null : "index.html"
  //     error_document           = length(var.redirect-all-requests-to) > 0 ? null : "error.html"
  //     redirect_all_requests_to = length(var.redirect-all-requests-to) > 0 ? var.redirect-all-requests-to : null
  //   }

  //   tags = {
  //     Name        = format("%s-s3-bucket", var.bucket-name)
  //     Environment = var.environment
  //     Stage       = var.stage
  //     Scope       = var.scope
  //   }
  // }

  // resource "aws_s3_bucket_policy" "s3-bucket-policy" {
  //   bucket = aws_s3_bucket.s3-bucket.id
  //   policy = data.aws_iam_policy_document.allow-cloudfront-read.json
  // }

  // data "aws_iam_policy_document" "allow-cloudfront-read" {

  //   statement {
  //     sid = "PublicReadGetObject"

  //     principals {
  //       type        = "*"
  //       identifiers = ["*"]
  //     }

  //     resources = [
  //       format("arn:aws:s3:::%s", var.bucket-name),
  //       format("arn:aws:s3:::%s/*", var.bucket-name)
  //     ]

  //     actions = ["s3:GetObject"]

  //     condition {
  //       test     = "IpAddress"
  //       variable = "aws:SourceIp"
  //       # https://www.cloudflare.com/ips-v4
  //       # https://www.cloudflare.com/ips-v6
  //       values = [
  //         "173.245.48.0/20",
  //         "103.21.244.0/22",
  //         "103.22.200.0/22",
  //         "103.31.4.0/22",
  //         "141.101.64.0/18",
  //         "108.162.192.0/18",
  //         "190.93.240.0/20",
  //         "188.114.96.0/20",
  //         "197.234.240.0/22",
  //         "198.41.128.0/17",
  //         "162.158.0.0/15",
  //         "104.16.0.0/12",
  //         "172.64.0.0/13",
  //         "131.0.72.0/22",
  //         "2400:cb00::/32",
  //         "2606:4700::/32",
  //         "2803:f800::/32",
  //         "2405:b500::/32",
  //         "2405:8100::/32",
  //         "2a06:98c0::/29",
  //         "2c0f:f248::/32",
  //       ]
  //     }
  //   }
  // }

  // module "static-website-www" {
  //   source = "./modules/static-website/"
  //   # S3 - Static Website
  //   acl                      = "private"
  //   bucket-name              = "www.lambdainsight.com"
  //   environment              = "website"
  //   redirect-all-requests-to = ""
  //   region                   = "eu-central-1"
  //   scope                    = "global"
  //   stage                    = "prod"
  // }



