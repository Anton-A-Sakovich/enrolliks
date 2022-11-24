const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
    mode: 'development',
    devtool: 'inline-source-map',

    entry: './src/index.js',

    output: {
        filename: '[name].js',
        // eslint-disable-next-line no-undef
        path: path.resolve(__dirname, '..', 'Enrolliks.Web', 'wwwroot'),
        clean: true,
        publicPath: '/',
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

    plugins: [
        new HtmlWebpackPlugin({
            title: 'Enrolliks',
        }),
    ],

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