// <TerminalPatterns>

module.exports.isAny = isAny;
function isAny() {
    return true;
}

module.exports.isConstant = isConstant;
function isConstant(constant) {
    return function isConstant$(value) {
        return Object.is(constant, value);
    }
}

module.exports.isBoolean = isBoolean;
function isBoolean(value) {
    return typeof(value) === 'boolean';
}

module.exports.isNumber = isNumber;
function isNumber(value) {
    return typeof(value) === 'number';
}

module.exports.isString = isString;
function isString(value) {
    return typeof(value) === 'string';
}

module.exports.isObject = isObject;
function isObject(value) {
    return !!value && typeof(value) === 'object' && !Array.isArray(value);
}

module.exports.isArray = isArray;
function isArray(value) {
   return Array.isArray(value);
}

// </TerminalPatterns>
// <CombinatorPatterns>

module.exports.not = not;
function not(pattern) {
    return function not$(value) {
        return !pattern(value);
    }
}

module.exports.and = and;
function and(...patterns) {
    return function and$(value) {
        for (const pattern of patterns) {
            if (!pattern(value)) return false;
        }

        return true;
    };
}

module.exports.or = or;
function or(...patterns) {
    return function or$(value) {
        for (const pattern of patterns) {
            if (pattern(value)) return true;
        }

        return false;
    };
}

// </CombinatorPatterns>
// <StructuralPatterns>

module.exports.hasProperty = hasProperty;
function hasProperty(name, pattern) {
    return function hasProperty$(value) {
        return (name in value) && pattern(value[name]);
    }
}

module.exports.repeated = repeated;
function repeated(pattern, min) {
    return function repeated$(values) {
        let valuesCount = 0;

        for (const value of values) {
            if (!pattern(value)) {
                return false;
            }

            valuesCount++;
        }

        return valuesCount >= min;
    }
}

// </StructuralPatterns>