import type {NextConfig} from 'next';
const config:NextConfig={output:'standalone',async rewrites(){return [{source:'/api/:path*',destination:`${process.env.INTERNAL_API_URL??'http://api:8080'}/api/:path*`}]}};export default config;
