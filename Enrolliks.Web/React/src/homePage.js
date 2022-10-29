const React = require('react');
const App = require('./app');

class HomePage extends React.Component {
    render() {
        return <p>Home</p>;
    }
}

App.render(<HomePage />);

module.exports = HomePage;