const React = require('react');

module.exports = class App extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            data: ("data" in window) ? window.data : null,
        };
    }

    render() {
        const Page = this.props.pageType;
        return (
            <>
                <h1>Enrolliks</h1>
                <nav className='top-nav'>
                    <a href='/home'>Home</a>
                    <a href='/people'>People</a>
                </nav>
                <div className='content-area'>
                    <Page args={this.state.data} />
                </div>
            </>
        );
    }
}