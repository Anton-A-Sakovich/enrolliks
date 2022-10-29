const path = require('path');

module.exports = {
    mode: 'development',
    devtool: 'inline-source-map',

    entry: {
        "home/index": './src/homePage.js',
        "people/index": './src/people/peoplePage.js',
        "people/create": './src/people/createPersonPage.js',
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
        runtimeChunk: 'single',
        moduleIds: 'deterministic',
        splitChunks: {
            cacheGroups: {
                vendor: {
                    test: /[\\/]node_modules[\\/]/,
                    name: 'vendors',
                    chunks: 'all',
                },
            },
        },
    },
};