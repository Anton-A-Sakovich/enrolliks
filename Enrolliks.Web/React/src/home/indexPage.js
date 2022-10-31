const React = require('react');
const App = require('../app');

class IndexPage extends React.Component {
    render() {
        return <p>There was a HOLE here. It's gone now.</p>;
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;