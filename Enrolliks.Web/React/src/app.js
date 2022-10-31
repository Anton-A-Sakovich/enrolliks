const React = require('react');
const { createRoot } = require('react-dom/client');

class App extends React.Component {
    static definePage(pageType) {
        if (!window.pageType) {
            window.pageType = pageType;

            const div = document.createElement('div');
            document.body.appendChild(div);

            const Page = pageType;
            createRoot(div).render(<App><Page /></App>);
        }
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