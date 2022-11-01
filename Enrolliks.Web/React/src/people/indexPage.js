const React = require('react');
const App = require('../app');
const CreatePersonComponent = require('./createPersonComponent');
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
                <div>
                    <CreatePersonComponent />
                </div>
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

class ErrorContent extends React.Component {
    render() {
        return <p>Error.</p>;
    }
}

class PeopleContent extends React.Component {
    render() {
        return (
        <div className='panel'>
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {this.props.people.map(person => <PersonRow key={person.name} person={person} />)}
                </tbody>
            </table>
        </div>);
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;