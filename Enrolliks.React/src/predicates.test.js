const {
    isAny, isConstant,
    isBoolean, isNumber, isString, isObject, isArray,
    and, or,
    hasProperty, repeated
} = require('./predicates');

// These values do not always follow typeof semantics or ECMA script specs, they follow
// what is convenient for parsing an HTTP response. For example, it is convenient to assume that
// isObject(null), isObject([]), isObject(() => 1)
// all return false despite typeof null being 'object' or an array / function being an object.
const values = {
    'null': [null],
    'undefined': [undefined],
    'boolean': [false, true],
    'number': [-1, +0, -0, 1, -1.1, 0.1, 1.1, NaN, +Infinity, -Infinity],
    'bigint': [-1n, 0n, 1n],
    'string': ["", "0", "a"],
    'object': [{}, { name: 'Max Caulfield'}],
    'array': [[], [1], [{}, 1]],
    'function': [() => 'Hello', class Foo {}],
};

function valuesOfType(...types) {
    let result = [];

    for (const type of types) {
        result = result.concat(values[type] || []);
    }

    return result;
}

function valuesExceptOfType(...typesToExclude) {
    const types = Object.keys(values).reduce(
        (accumulator, value) =>
            typesToExclude.indexOf(value) === -1 ? accumulator.concat(value) : accumulator,
        []);

    return valuesOfType(...types);
}

function createMockedFunction(...returnValues) {
    const mockedFunction = jest.fn();

    if (returnValues.length === 0) {
        mockedFunction.mockReturnValue(undefined);
    } else {
        returnValues.reduce((accumulator, value, index) => {
            if (index === returnValues.length - 1) {
                return accumulator.mockReturnValue(value);
            } else {
                return accumulator.mockReturnValueOnce(value);
            }
        }, mockedFunction);
    }

    return mockedFunction;
}

describe('isAny', () => {
    it('is atomic predicate', () => {
        expect(typeof isAny({})).toBe('boolean');
    });

    it.each(valuesExceptOfType())('is true for %p', (value) => {
        expect(isAny(value)).toBe(true);
    });
});

describe('isConstant', () => {
    const strictlyNotEqualValues = [
        [1, "1"],
        [0, false],
        ["", false],
        [[], []],
        [{}, {}]
    ];

    it('requires a constant', () => {
        expect(typeof isConstant(1)(1)).toBe('boolean');
    });

    it.each(strictlyNotEqualValues)('uses strict equality', (constant, value) => {
        expect(isConstant(constant)(value)).toBe(false);
    });

    it.each(valuesExceptOfType())('returns true for %p', (value) => {
        expect(isConstant(value)(value)).toBe(true);
    });
});

const typePredicatesSpecs = [
    [isBoolean, valuesOfType('boolean'), valuesExceptOfType('boolean')],
    [isNumber, valuesOfType('number'), valuesExceptOfType('number')],
    [isString, valuesOfType('string'), valuesExceptOfType('string')],
    [isObject, valuesOfType('object'), valuesExceptOfType('object')],
    [isArray, valuesOfType('array'), valuesExceptOfType('array')],
];

describe.each(typePredicatesSpecs)('%p', (predicate, trueValues, falseValues) => {
    it('is atomic predicate', () => {
        expect(typeof predicate(trueValues[0])).toBe('boolean');
    });

    it.each(trueValues.map(value => [value]))('is true for %p', (value) => {
        expect(predicate(value)).toBe(true);
    });

    it.each(falseValues.map(value => [value]))('is false for %p', (value) => {
        expect(predicate(value)).toBe(false);
    });
})

describe('and', () => {
    it('is composite predicate', () => {
        const predicate1 = createMockedFunction(true);
        const predicate2 = createMockedFunction(true);

        expect(typeof and(predicate1, predicate2)(1)).toBe('boolean');
    })

    it('returns true when empty', () => {
        expect(and()(1)).toBe(true);
    });

    const algebra = [
        {
            predicate1Returns: false,
            predicate2Returns: false,
            result: false,
            predicate1Called: true,
            predicate2Called: false,
        },
        {
            predicate1Returns: false,
            predicate2Returns: true,
            result: false,
            predicate1Called: true,
            predicate2Called: false,
        },
        {
            predicate1Returns: true,
            predicate2Returns: false,
            result: false,
            predicate1Called: true,
            predicate2Called: true,
        },
        {
            predicate1Returns: true,
            predicate2Returns: true,
            result: true,
            predicate1Called: true,
            predicate2Called: true,
        }
    ];

    it.each(algebra)('obeys lazy algebra ($predicate1Returns and $predicate2Returns === $result)', algebra => {
        const predicate1 = createMockedFunction(algebra.predicate1Returns);
        const predicate2 = createMockedFunction(algebra.predicate2Returns);
        const value = { name: 'Max Caulfield' };

        expect(and(predicate1, predicate2)(value)).toBe(algebra.result);
        expect(predicate1.mock.calls).toEqual(algebra.predicate1Called ? [[value]] : []);
        expect(predicate2.mock.calls).toEqual(algebra.predicate2Called ? [[value]] : []);
    });
});

describe('or', () => {
    it('is composite predicate', () => {
        const predicate1 = createMockedFunction(false);
        const predicate2 = createMockedFunction(false);

        expect(typeof or(predicate1, predicate2)(1)).toBe('boolean');
    })

    it('returns false when empty', () => {
        expect(or()(1)).toBe(false);
    });

    const algebra = [
        {
            predicate1Returns: false,
            predicate2Returns: false,
            result: false,
            predicate1Called: true,
            predicate2Called: true,
        },
        {
            predicate1Returns: false,
            predicate2Returns: true,
            result: true,
            predicate1Called: true,
            predicate2Called: true,
        },
        {
            predicate1Returns: true,
            predicate2Returns: false,
            result: true,
            predicate1Called: true,
            predicate2Called: false,
        },
        {
            predicate1Returns: true,
            predicate2Returns: true,
            result: true,
            predicate1Called: true,
            predicate2Called: false,
        }
    ];

    it.each(algebra)('obeys lazy algebra ($predicate1Returns or $predicate2Returns === $result)', algebra => {
        const predicate1 = createMockedFunction(algebra.predicate1Returns);
        const predicate2 = createMockedFunction(algebra.predicate2Returns);
        const value = { name: 'Max Caulfield' };

        expect(or(predicate1, predicate2)(value)).toBe(algebra.result);
        expect(predicate1.mock.calls).toEqual(algebra.predicate1Called ? [[value]] : []);
        expect(predicate2.mock.calls).toEqual(algebra.predicate2Called ? [[value]] : []);
    });
});

describe('hasProperty', () => {
    it('is composite predicate', () => {
        const propertyValuePredicate = createMockedFunction(true);
        const propertyName = 'name';
        const value = {[propertyName]: 'Max Caulfield'};

        expect(typeof hasProperty(propertyName, propertyValuePredicate)(value)).toBe('boolean');
    });

    const algebra = [
        {
            propertyExists: false,
            propertyPredicatesReturns: false,
            result: false,
            propertyPredicatesCalled: false,
        },
        {
            propertyExists: false,
            propertyPredicatesReturns: true,
            result: false,
            propertyPredicatesCalled: false,
        },
        {
            propertyExists: true,
            propertyPredicatesReturns: false,
            result: false,
            propertyPredicatesCalled: true,
        },
        {
            propertyExists: true,
            propertyPredicatesReturns: true,
            result: true,
            propertyPredicatesCalled: true,
        }
    ];

    it.each(algebra)('lazily checks the property predicate (exists = $propertyExists, matches = $propertyPredicatesReturns)',
    ({propertyExists, propertyPredicatesReturns, result, propertyPredicatesCalled}) => {
        const propertyValuePredicate = createMockedFunction(propertyPredicatesReturns);
        const propertyName = 'name';
        const propertyValue = 'Max Caulfield';
        const value = propertyExists ? { [propertyName]: propertyValue } : {};

        expect(hasProperty(propertyName, propertyValuePredicate)(value)).toBe(result);
        expect(propertyValuePredicate.mock.calls).toEqual(propertyPredicatesCalled ? [[propertyValue]] : []);
    });
});

describe('repeated', () => {
    it('is composite predicate', () => {
        const elementPredicate = createMockedFunction(true);
        expect(typeof repeated(elementPredicate, 1)(["Max"])).toBe('boolean');
    });

    function createSequence(length) {
        return {
            *[Symbol.iterator]() {
                for (let i = 0; i < length; i++) {
                    yield i;
                }
            }
        };
    }

    function sequenceMap(fun) {
        const originalSequence = this;
        return {
            *[Symbol.iterator]() {
                for (const value of originalSequence) {
                    yield fun(value);
                }
            }
        };
    }

    function createSequenceWithEnhancements(length) {
        const sequence = createSequence(length);
        sequence.length = length;
        sequence.map = sequenceMap;
        return sequence;
    }

    const sequences = [
        {sequence: createSequenceWithEnhancements(0)},
        {sequence: createSequenceWithEnhancements(1)},
        {sequence: createSequenceWithEnhancements(2)},
        {sequence: createSequenceWithEnhancements(3)},
        {sequence: createSequenceWithEnhancements(4)},
    ];

    it.each(sequences)('checks that there are at least minimum elements', ({sequence}) => {
        const elementPredicate = createMockedFunction(true);
        const minimum = 2;
        expect(repeated(elementPredicate, minimum)(sequence)).toBe(sequence.length >= minimum);
    });

    it.each(sequences)('checks that all the elements match', ({sequence}) => {
        const alwaysTruePredicate = createMockedFunction(true);
        expect(repeated(alwaysTruePredicate, 0)(sequence)).toBe(true);
        expect(alwaysTruePredicate.mock.calls).toEqual(Array.from(sequence.map(value => [value])));

        const sometimesTruePredicate = createMockedFunction(true, true, false);
        expect(repeated(sometimesTruePredicate, 0)(sequence)).toBe(sequence.length < 3);
    });
});