const React = require('react');
const App = require('../app');
const PersonRow = require('./personRow');

class IndexPage extends React.Component {
    constructor(props) {
        super(props);

        let content;
        if (!window.data || !window.data?.length)
            content = <ErrorContent />;
        else if (window.data.length === 0)
            content = <EmptyContent />;
        else
            content = <PeopleContent people={window.data} />;

        this.state = {
            content: content,
        };
    }

    render() {
        return (
            <div>
                <a href='/people/create'>Create new...</a>
                <div>
                    {this.state.content}
                </div>
            </div>
        );
    }
}

class EmptyContent extends React.Component {
    render() {
        return <p>No people found.</p>;
    }
}

class PeopleContent extends React.Component {
    render() {
        return (<table>
            <thead>
                <tr>
                    <th>Name</th>
                </tr>
            </thead>
            <tbody>
                {this.props.people.map(person => <PersonRow person={person} />)}
            </tbody>
        </table>);
    }
}

class ErrorContent extends React.Component {
    render() {
        return <p>Error.</p>;
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;