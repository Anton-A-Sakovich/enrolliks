const React = require('react');
const { createRoot } = require('react-dom/client');

const HomePage = require('./homePage');
const PeoplePage = require('./people/peoplePage');

class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            currentPage: <HomePage />
        };
    }

    navigateTo(page) {
        this.setState({
            currentPage: page,
        });
    }

    render() {
        return (
            <div>
                <h1>Enrolliks</h1>
                <nav>
                    <button onClick={() => this.navigateTo(<HomePage />)}>Home</button>
                    <button onClick={() => this.navigateTo(<PeoplePage navigateTo={page => this.navigateTo(page)} />)}>People</button>
                </nav>
                {this.state.currentPage}
            </div>
        );
    }
}

const div = document.createElement('div');
document.body.appendChild(div);
createRoot(div).render(<App />);