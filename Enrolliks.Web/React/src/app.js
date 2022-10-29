const React = require('react');
const { createRoot } = require('react-dom/client');

class App extends React.Component {
    static render(page) {
        const div = document.createElement('div');
        document.body.appendChild(div);

        const app = <App>{page}</App>;

        createRoot(div).render(app);
    }

    render() {
        return (
            <div>
                <h1>Enrolliks</h1>
                <nav>
                    <a href='/home'>Home</a>
                    <a href='/people'>People</a>
                </nav>
                {this.props.children}
            </div>
        );
    }
}

module.exports = App;