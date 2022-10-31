const React = require('react');
const App = require('../app');

class IndexPage extends React.Component {
    render() {
        return <p>Home</p>;
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;