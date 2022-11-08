const React = require('react');
const CreatePersonForm = require('./createPersonForm');
const PersonRow = require('./personRow');

const success = 0;
const failure = 1;

module.exports = class PeoplePage extends React.Component {
    constructor(props) {
        super(props);

        let content;
        const args = this.props.args;
        if (args === null || !('type' in args) || args.type === failure)
            content = <ErrorContent />;
        else if (args.type === success)
            content = args.people.length === 0
                ? <EmptyContent />
                : <PeopleContent people={args.people} />;
        else
            content = <ErrorContent />;

        this.state = {
            content: content,
        };
    }

    render() {
        return (
            <div>
                <div>
                    <CreatePersonForm />
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