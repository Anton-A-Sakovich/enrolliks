﻿const path = require('path');

module.exports = {
    mode: 'development',
    devtool: 'inline-source-map',

    entry: {
        "home/index": './src/home/indexPage.js',
        "people/index": './src/people/indexPage.js',
        "people/create": './src/people/createPage.js',
    },

    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, '..', 'wwwroot', 'scripts'),
        clean: true,
    },

    module: {
        rules: [
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.jsx?$/i,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [
                            [
                                '@babel/preset-react',
                                {
                                    targets: 'defaults',
                                    runtime: 'automatic',
                                },
                            ],
                        ],
                        plugins: [
                            ['@babel/plugin-transform-modules-commonjs']
                        ],
                    },
                },
            }
        ],
    },

    optimization: {
        splitChunks: {
            cacheGroups: {
                vendors: {
                    test: /[\\/]node_modules[\\/]/,
                    name: 'vendors',
                    chunks: 'all',
                    enforce: true,
                },
                app: {
                    test: /[\\/]src[\\/]app\.js/,
                    name: 'app',
                    chunks: 'all',
                    enforce: true,
                },
            },
        },
    },
};