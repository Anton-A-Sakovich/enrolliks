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

class ErrorContent extends React.Component {
    render() {
        return <p>Error.</p>;
    }
}

class CreatePersonComponent extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            editing: false,
            name: '',
        };
    }

    render() {
        if (!this.state.editing) {
            return <button onClick={() => this.handleCreateClick()}>Create</button>;
        } else {
            return (
            <form>
                <div className="panel">
                    <div className='form-row'>
                        <label className='form-label' htmlFor="name">Name</label>
                        <input className='form-input' type="text" id="name" name="name" value={this.state.name} onChange={event => this.handleNameChange(event)} />
                    </div>
                    <div>
                        <button onClick={() => this.handleSubmitClick()}>Create</button>
                        <button onClick={() => this.handleCancelClick()}>Cancel</button>
                    </div>
                </div>
            </form>);
        }
    }

    handleNameChange(event) {
        this.setState({
            name: event.target.value,
        });
    }

    handleCreateClick() {
        this.setState({
            editing: true,
        });
    }

    handleCancelClick() {
        this.setState({
            editing: false,
            name: '',
        });
    }

    handleSubmitClick() {
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;